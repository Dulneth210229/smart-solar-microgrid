/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: BookingSlotService.cs
 * Description: Handles booking slot management and scheduling business rules.
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class BookingSlotService
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes booking-slot database access.
        public BookingSlotService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Returns active future booking slots for an active solar station.
        public async Task<List<EnergyBookingSlot>>
            GetAvailableSlotsForStationAsync(string stationId)
        {
            ValidateObjectId(stationId, "station");

            var station = await _mongoDbService.Stations
                .Find(item => item.Id == stationId)
                .FirstOrDefaultAsync();

            if (station == null)
            {
                throw new InvalidOperationException(
                    "Solar station not found."
                );
            }

            if (!station.IsActive)
            {
                return new List<EnergyBookingSlot>();
            }

            DateTime nowUtc = DateTime.UtcNow;

            return await _mongoDbService.BookingSlots
                .Find(slot =>
                    slot.StationId == stationId &&
                    slot.IsActive &&
                    slot.StartTimeUtc > nowUtc)
                .SortBy(slot => slot.StartTimeUtc)
                .ToListAsync();
        }

        // Returns all booking slots for station-management purposes.
        public async Task<List<EnergyBookingSlot>>
            GetAllSlotsForStationAsync(string stationId)
        {
            ValidateObjectId(stationId, "station");

            return await _mongoDbService.BookingSlots
                .Find(slot => slot.StationId == stationId)
                .SortBy(slot => slot.StartTimeUtc)
                .ToListAsync();
        }

        // Returns one booking slot using its MongoDB ObjectId.
        public async Task<EnergyBookingSlot?> GetByIdAsync(
            string id)
        {
            ValidateObjectId(id, "booking slot");

            return await _mongoDbService.BookingSlots
                .Find(slot => slot.Id == id)
                .FirstOrDefaultAsync();
        }

        // Creates a new booking slot after validating station and schedule rules.
        public async Task<EnergyBookingSlot> CreateAsync(
            CreateBookingSlotRequest request)
        {
            ValidateObjectId(request.StationId, "station");

            var station = await _mongoDbService.Stations
                .Find(item => item.Id == request.StationId)
                .FirstOrDefaultAsync();

            if (station == null)
            {
                throw new InvalidOperationException(
                    "Solar station not found."
                );
            }

            if (!station.IsActive)
            {
                throw new InvalidOperationException(
                    "Booking slots cannot be created for a deactivated station."
                );
            }

            DateTime startUtc = request.StartTime.UtcDateTime;
            DateTime endUtc = request.EndTime.UtcDateTime;

            ValidateSlotSchedule(
                station,
                startUtc,
                endUtc,
                request.CapacitySlots
            );

            bool overlaps = await HasOverlapAsync(
                request.StationId,
                startUtc,
                endUtc,
                null
            );

            if (overlaps)
            {
                throw new InvalidOperationException(
                    "The booking slot overlaps an existing active slot."
                );
            }

            var bookingSlot = new EnergyBookingSlot
            {
                StationId = request.StationId,
                StartTimeUtc = startUtc,
                EndTimeUtc = endUtc,
                CapacitySlots = request.CapacitySlots,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.BookingSlots
                .InsertOneAsync(bookingSlot);

            return bookingSlot;
        }

        // Updates an unused booking slot after validating schedule rules.
        public async Task<EnergyBookingSlot?> UpdateAsync(
            string id,
            UpdateBookingSlotRequest request)
        {
            ValidateObjectId(id, "booking slot");

            var existing = await GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            bool reservationsExist =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.SlotId == id)
                    .AnyAsync();

            if (reservationsExist)
            {
                throw new InvalidOperationException(
                    "This booking slot cannot be changed because reservations already reference it."
                );
            }

            var station = await _mongoDbService.Stations
                .Find(item => item.Id == existing.StationId)
                .FirstOrDefaultAsync();

            if (station == null)
            {
                throw new InvalidOperationException(
                    "The booking slot's solar station no longer exists."
                );
            }

            if (!station.IsActive)
            {
                throw new InvalidOperationException(
                    "Booking slots cannot be updated while the station is deactivated."
                );
            }

            DateTime startUtc = request.StartTime.UtcDateTime;
            DateTime endUtc = request.EndTime.UtcDateTime;

            ValidateSlotSchedule(
                station,
                startUtc,
                endUtc,
                request.CapacitySlots
            );

            bool overlaps = await HasOverlapAsync(
                existing.StationId,
                startUtc,
                endUtc,
                id
            );

            if (overlaps)
            {
                throw new InvalidOperationException(
                    "The updated booking slot overlaps an existing active slot."
                );
            }

            var update = Builders<EnergyBookingSlot>.Update
                .Set(
                    slot => slot.StartTimeUtc,
                    startUtc
                )
                .Set(
                    slot => slot.EndTimeUtc,
                    endUtc
                )
                .Set(
                    slot => slot.CapacitySlots,
                    request.CapacitySlots
                )
                .Set(
                    slot => slot.UpdatedAtUtc,
                    DateTime.UtcNow
                );

            await _mongoDbService.BookingSlots
                .UpdateOneAsync(
                    slot => slot.Id == id,
                    update
                );

            return await GetByIdAsync(id);
        }

        // Deactivates a booking slot when it has no active reservations.
        public async Task<bool> DeactivateAsync(string id)
        {
            ValidateObjectId(id, "booking slot");

            var slot = await GetByIdAsync(id);

            if (slot == null)
            {
                return false;
            }

            bool activeReservationsExist =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.SlotId == id &&
                        (
                            reservation.Status ==
                                ReservationStatuses.Pending ||

                            reservation.Status ==
                                ReservationStatuses.Approved
                        ))
                    .AnyAsync();

            if (activeReservationsExist)
            {
                throw new InvalidOperationException(
                    "The booking slot cannot be deactivated because active reservations exist."
                );
            }

            var result = await _mongoDbService.BookingSlots
                .UpdateOneAsync(
                    item => item.Id == id,

                    Builders<EnergyBookingSlot>.Update
                        .Set(item => item.IsActive, false)
                        .Set(
                            item => item.UpdatedAtUtc,
                            DateTime.UtcNow
                        )
                );

            return result.MatchedCount > 0;
        }

        // Reactivates a booking slot if its parent station is active.
        public async Task<bool> ActivateAsync(string id)
        {
            ValidateObjectId(id, "booking slot");

            var slot = await GetByIdAsync(id);

            if (slot == null)
            {
                return false;
            }

            var station = await _mongoDbService.Stations
                .Find(item => item.Id == slot.StationId)
                .FirstOrDefaultAsync();

            if (station == null || !station.IsActive)
            {
                throw new InvalidOperationException(
                    "The booking slot cannot be activated because its solar station is inactive or missing."
                );
            }

            var result = await _mongoDbService.BookingSlots
                .UpdateOneAsync(
                    item => item.Id == id,

                    Builders<EnergyBookingSlot>.Update
                        .Set(item => item.IsActive, true)
                        .Set(
                            item => item.UpdatedAtUtc,
                            DateTime.UtcNow
                        )
                );

            return result.MatchedCount > 0;
        }

        // Permanently deletes an unused booking slot.
        public async Task<bool> DeleteAsync(string id)
        {
            ValidateObjectId(id, "booking slot");

            bool reservationsExist =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.SlotId == id)
                    .AnyAsync();

            if (reservationsExist)
            {
                throw new InvalidOperationException(
                    "The booking slot cannot be deleted because reservation history references it."
                );
            }

            var result =
                await _mongoDbService.BookingSlots
                    .DeleteOneAsync(slot => slot.Id == id);

            return result.DeletedCount > 0;
        }

        // Checks whether another active slot overlaps the requested time period.
        private async Task<bool> HasOverlapAsync(
            string stationId,
            DateTime startUtc,
            DateTime endUtc,
            string? excludedSlotId)
        {
            var filter =
                Builders<EnergyBookingSlot>.Filter.Eq(
                    slot => slot.StationId,
                    stationId
                )
                &
                Builders<EnergyBookingSlot>.Filter.Eq(
                    slot => slot.IsActive,
                    true
                )
                &
                Builders<EnergyBookingSlot>.Filter.Lt(
                    slot => slot.StartTimeUtc,
                    endUtc
                )
                &
                Builders<EnergyBookingSlot>.Filter.Gt(
                    slot => slot.EndTimeUtc,
                    startUtc
                );

            if (!string.IsNullOrWhiteSpace(excludedSlotId))
            {
                filter &=
                    Builders<EnergyBookingSlot>.Filter.Ne(
                        slot => slot.Id,
                        excludedSlotId
                    );
            }

            return await _mongoDbService.BookingSlots
                .Find(filter)
                .AnyAsync();
        }

        // Validates time, capacity and station operating-schedule rules.
        private static void ValidateSlotSchedule(
            SolarStation station,
            DateTime startUtc,
            DateTime endUtc,
            int capacitySlots)
        {
            if (startUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Booking slot start time must be in the future."
                );
            }

            if (endUtc <= startUtc)
            {
                throw new InvalidOperationException(
                    "Booking slot end time must be later than its start time."
                );
            }

            if (capacitySlots > station.TotalBatterySlots)
            {
                throw new InvalidOperationException(
                    $"Booking slot capacity cannot exceed the station's {station.TotalBatterySlots} battery slots."
                );
            }

            TimeZoneInfo sriLankaTimeZone =
                GetSriLankaTimeZone();

            DateTime localStart =
                TimeZoneInfo.ConvertTimeFromUtc(
                    startUtc,
                    sriLankaTimeZone
                );

            DateTime localEnd =
                TimeZoneInfo.ConvertTimeFromUtc(
                    endUtc,
                    sriLankaTimeZone
                );

            if (localStart.Date != localEnd.Date)
            {
                throw new InvalidOperationException(
                    "A booking slot must start and end on the same day."
                );
            }

            string day =
                localStart.DayOfWeek
                    .ToString()
                    .ToUpperInvariant();

            if (!station.OperatingDays.Contains(day))
            {
                throw new InvalidOperationException(
                    $"The station does not operate on {day}."
                );
            }

            TimeOnly openingTime =
                TimeOnly.Parse(station.OpeningTime);

            TimeOnly closingTime =
                TimeOnly.Parse(station.ClosingTime);

            TimeOnly slotStart =
                TimeOnly.FromDateTime(localStart);

            TimeOnly slotEnd =
                TimeOnly.FromDateTime(localEnd);

            if (slotStart < openingTime ||
                slotEnd > closingTime)
            {
                throw new InvalidOperationException(
                    $"Booking slots must be within station operating hours {station.OpeningTime} - {station.ClosingTime}."
                );
            }
        }

        // Returns the Sri Lankan timezone for schedule validation.
        private static TimeZoneInfo GetSriLankaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Sri Lanka Standard Time"
                );
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Asia/Colombo"
                );
            }
        }

        // Verifies that an identifier is a valid MongoDB ObjectId.
        private static void ValidateObjectId(
            string id,
            string objectName)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                throw new ArgumentException(
                    $"Invalid {objectName} ID."
                );
            }
        }
    }
}