/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: SolarStationService.cs
 * Description: Handles solar microgrid station management and business rules.
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class SolarStationService
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes station-management database access.
        public SolarStationService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Returns all active solar stations for application users.
        public async Task<List<SolarStation>> GetActiveStationsAsync()
        {
            return await _mongoDbService.Stations
                .Find(station => station.IsActive)
                .ToListAsync();
        }

        // Returns all stations including deactivated stations for Backoffice management.
        public async Task<List<SolarStation>> GetAllStationsAsync()
        {
            return await _mongoDbService.Stations
                .Find(_ => true)
                .ToListAsync();
        }

        // Returns a station using its MongoDB ObjectId.
        public async Task<SolarStation?> GetByIdAsync(string id)
        {
            ValidateObjectId(id);

            return await _mongoDbService.Stations
                .Find(station => station.Id == id)
                .FirstOrDefaultAsync();
        }

        // Creates a new active solar microgrid station.
        public async Task<SolarStation> CreateAsync(
            CreateSolarStationRequest request)
        {
            ValidateStationSchedule(
                request.OpeningTime,
                request.ClosingTime,
                request.OperatingDays
            );

            var duplicateName = await _mongoDbService.Stations
                .Find(station =>
                    station.Name.ToLower() ==
                    request.Name.Trim().ToLower())
                .FirstOrDefaultAsync();

            if (duplicateName != null)
            {
                throw new InvalidOperationException(
                    "A solar station with this name already exists."
                );
            }

            var station = new SolarStation
            {
                Name = request.Name.Trim(),
                Location = request.Location.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CapacityKw = request.CapacityKw,
                BatteryCapacityKwh = request.BatteryCapacityKwh,
                TotalBatterySlots = request.TotalBatterySlots,
                AvailableBatterySlots = request.TotalBatterySlots,

                OpeningTime =
                    NormalizeTime(request.OpeningTime),

                ClosingTime =
                    NormalizeTime(request.ClosingTime),

                OperatingDays =
                    NormalizeOperatingDays(request.OperatingDays),

                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.Stations.InsertOneAsync(station);

            return station;
        }

        // Updates station details while preserving occupied battery-slot information.
        public async Task<SolarStation?> UpdateAsync(
            string id,
            UpdateSolarStationRequest request)
        {
            ValidateObjectId(id);

            ValidateStationSchedule(
                request.OpeningTime,
                request.ClosingTime,
                request.OperatingDays
            );

            var existing = await GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            var duplicateName = await _mongoDbService.Stations
                .Find(station =>
                    station.Id != id &&
                    station.Name.ToLower() ==
                    request.Name.Trim().ToLower())
                .FirstOrDefaultAsync();

            if (duplicateName != null)
            {
                throw new InvalidOperationException(
                    "Another solar station already uses this name."
                );
            }

            int occupiedSlots =
                existing.TotalBatterySlots -
                existing.AvailableBatterySlots;

            if (request.TotalBatterySlots < occupiedSlots)
            {
                throw new InvalidOperationException(
                    $"Total battery slots cannot be lower than the {occupiedSlots} currently occupied slots."
                );
            }

            int newAvailableSlots =
                request.TotalBatterySlots - occupiedSlots;

            var update = Builders<SolarStation>.Update
                .Set(station => station.Name, request.Name.Trim())
                .Set(station => station.Location, request.Location.Trim())
                .Set(station => station.Latitude, request.Latitude)
                .Set(station => station.Longitude, request.Longitude)
                .Set(station => station.CapacityKw, request.CapacityKw)
                .Set(
                    station => station.BatteryCapacityKwh,
                    request.BatteryCapacityKwh
                )
                .Set(
                    station => station.TotalBatterySlots,
                    request.TotalBatterySlots
                )
                .Set(
                    station => station.AvailableBatterySlots,
                    newAvailableSlots
                )
                .Set(
                    station => station.OpeningTime,
                    NormalizeTime(request.OpeningTime)
                )
                .Set(
                    station => station.ClosingTime,
                    NormalizeTime(request.ClosingTime)
                )
                .Set(
                    station => station.OperatingDays,
                    NormalizeOperatingDays(request.OperatingDays)
                )
                .Set(
                    station => station.UpdatedAtUtc,
                    DateTime.UtcNow
                );

            await _mongoDbService.Stations.UpdateOneAsync(
                station => station.Id == id,
                update
            );

            return await GetByIdAsync(id);
        }

        // Updates the number of currently available battery slots.
        public async Task<SolarStation?> UpdateAvailabilityAsync(
            string id,
            int availableBatterySlots)
        {
            ValidateObjectId(id);

            var existing = await GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            if (!existing.IsActive)
            {
                throw new InvalidOperationException(
                    "Availability cannot be updated for a deactivated station."
                );
            }

            if (availableBatterySlots >
                existing.TotalBatterySlots)
            {
                throw new InvalidOperationException(
                    $"Available battery slots cannot exceed the total of {existing.TotalBatterySlots}."
                );
            }

            var update = Builders<SolarStation>.Update
                .Set(
                    station => station.AvailableBatterySlots,
                    availableBatterySlots
                )
                .Set(
                    station => station.UpdatedAtUtc,
                    DateTime.UtcNow
                );

            await _mongoDbService.Stations.UpdateOneAsync(
                station => station.Id == id,
                update
            );

            return await GetByIdAsync(id);
        }

        // Deactivates a station only when no active reservations exist.
        public async Task<bool> DeactivateAsync(string id)
        {
            ValidateObjectId(id);

            var station = await GetByIdAsync(id);

            if (station == null)
            {
                return false;
            }

            bool activeReservationsExist =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.StationId == id &&
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
                    "The station cannot be deactivated because active energy reservations exist."
                );
            }

            var result =
                await _mongoDbService.Stations.UpdateOneAsync(
                    item => item.Id == id,

                    Builders<SolarStation>.Update
                        .Set(item => item.IsActive, false)
                        .Set(
                            item => item.UpdatedAtUtc,
                            DateTime.UtcNow
                        )
                );

            return result.MatchedCount > 0;
        }

        // Reactivates a previously deactivated station.
        public async Task<bool> ActivateAsync(string id)
        {
            ValidateObjectId(id);

            var result =
                await _mongoDbService.Stations.UpdateOneAsync(
                    station => station.Id == id,

                    Builders<SolarStation>.Update
                        .Set(station => station.IsActive, true)
                        .Set(
                            station => station.UpdatedAtUtc,
                            DateTime.UtcNow
                        )
                );

            return result.MatchedCount > 0;
        }

        // Verifies that a station ID is a valid MongoDB ObjectId.
        private static void ValidateObjectId(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                throw new ArgumentException(
                    "Invalid solar station ID."
                );
            }
        }

        // Validates operating hours and supported operating days.
        private static void ValidateStationSchedule(
            string openingTime,
            string closingTime,
            List<string> operatingDays)
        {
            if (!TimeOnly.TryParse(
                    openingTime,
                    out TimeOnly opening))
            {
                throw new InvalidOperationException(
                    "Opening time is invalid. Use a value such as 08:00."
                );
            }

            if (!TimeOnly.TryParse(
                    closingTime,
                    out TimeOnly closing))
            {
                throw new InvalidOperationException(
                    "Closing time is invalid. Use a value such as 18:00."
                );
            }

            if (closing <= opening)
            {
                throw new InvalidOperationException(
                    "Closing time must be later than opening time."
                );
            }

            if (operatingDays == null ||
                operatingDays.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one operating day is required."
                );
            }

            string[] validDays =
            {
                "MONDAY",
                "TUESDAY",
                "WEDNESDAY",
                "THURSDAY",
                "FRIDAY",
                "SATURDAY",
                "SUNDAY"
            };

            foreach (string day in operatingDays)
            {
                if (!validDays.Contains(
                        day.Trim().ToUpperInvariant()))
                {
                    throw new InvalidOperationException(
                        $"Invalid operating day: {day}."
                    );
                }
            }
        }

        // Normalizes an operating time into HH:mm format.
        private static string NormalizeTime(string value)
        {
            TimeOnly time = TimeOnly.Parse(value);

            return time.ToString("HH:mm");
        }

        // Normalizes operating-day values and removes duplicates.
        private static List<string> NormalizeOperatingDays(
            List<string> days)
        {
            return days
                .Select(day =>
                    day.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();
        }
    }
}