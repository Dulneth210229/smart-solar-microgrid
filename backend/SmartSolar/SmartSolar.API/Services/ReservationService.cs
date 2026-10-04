/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ReservationService.cs
 * Description: Handles energy reservation creation, modification, cancellation and approval.
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;
using System.Security.Cryptography;

namespace SmartSolar.API.Services
{
    public class ReservationService
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes reservation database access.
        public ReservationService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Returns all reservations belonging to the authenticated Prosumer.
        public async Task<List<ReservationResponse>>
            GetMyReservationsAsync(string prosumerNic)
        {
            var reservations = await _mongoDbService.Reservations
                .Find(reservation =>
                    reservation.ProsumerNic == prosumerNic)
                .SortByDescending(reservation =>
                    reservation.CreatedAtUtc)
                .ToListAsync();

            var responses = new List<ReservationResponse>();

            foreach (var reservation in reservations)
            {
                responses.Add(
                    await BuildResponseAsync(reservation)
                );
            }

            return responses;
        }

        // Returns all reservations currently waiting for staff approval.
        public async Task<List<ReservationResponse>>
            GetPendingReservationsAsync()
        {
            var reservations = await _mongoDbService.Reservations
                .Find(reservation =>
                    reservation.Status ==
                    ReservationStatuses.Pending)
                .SortBy(reservation =>
                    reservation.CreatedAtUtc)
                .ToListAsync();

            var responses = new List<ReservationResponse>();

            foreach (var reservation in reservations)
            {
                responses.Add(
                    await BuildResponseAsync(reservation)
                );
            }

            return responses;
        }

        // Returns one reservation from MongoDB.
        public async Task<EnergyReservation?>
            GetReservationByIdAsync(string id)
        {
            ValidateObjectId(id, "reservation");

            return await _mongoDbService.Reservations
                .Find(reservation =>
                    reservation.Id == id)
                .FirstOrDefaultAsync();
        }

        // Returns one reservation as client-friendly response data.
        public async Task<ReservationResponse?>
            GetResponseByIdAsync(string id)
        {
            var reservation =
                await GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return null;
            }

            return await BuildResponseAsync(reservation);
        }

        // Creates a pending reservation for an active Prosumer.
        public async Task<ReservationResponse> CreateAsync(
            string prosumerNic,
            CreateReservationRequest request)
        {
            await EnsureActiveProsumerAsync(prosumerNic);

            string transferType =
                NormalizeTransferType(request.TransferType);

            var target = await ValidateReservationTargetAsync(
                request.SlotId,
                request.EnergyAmountKwh,
                null
            );

            bool duplicateReservation =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.ProsumerNic ==
                            prosumerNic &&
                        reservation.SlotId ==
                            request.SlotId &&
                        (
                            reservation.Status ==
                                ReservationStatuses.Pending ||
                            reservation.Status ==
                                ReservationStatuses.Approved
                        ))
                    .AnyAsync();

            if (duplicateReservation)
            {
                throw new InvalidOperationException(
                    "You already have an active reservation for this booking slot."
                );
            }

            var reservation = new EnergyReservation
            {
                ProsumerNic = prosumerNic,
                StationId = target.Slot.StationId,
                SlotId = target.Slot.Id!,
                EnergyAmountKwh =
                    request.EnergyAmountKwh,

                TransferType = transferType,

                Status = ReservationStatuses.Pending,

                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.Reservations
                .InsertOneAsync(reservation);

            return await BuildResponseAsync(reservation);
        }

        // Updates a Prosumer reservation when at least 12 hours remain.
        public async Task<ReservationResponse?> UpdateAsync(
            string prosumerNic,
            string id,
            UpdateReservationRequest request)
        {
            ValidateObjectId(id, "reservation");

            await EnsureActiveProsumerAsync(prosumerNic);

            var existing = await _mongoDbService.Reservations
                .Find(reservation =>
                    reservation.Id == id &&
                    reservation.ProsumerNic ==
                        prosumerNic)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                return null;
            }

            if (existing.Status !=
                    ReservationStatuses.Pending &&
                existing.Status !=
                    ReservationStatuses.Approved)
            {
                throw new InvalidOperationException(
                    "Only pending or approved reservations can be modified."
                );
            }

            var existingSlot =
                await _mongoDbService.BookingSlots
                    .Find(slot =>
                        slot.Id == existing.SlotId)
                    .FirstOrDefaultAsync();

            if (existingSlot == null)
            {
                throw new InvalidOperationException(
                    "The reservation's booking slot could not be found."
                );
            }

            EnsureTwelveHourNotice(existingSlot);

            string transferType =
                NormalizeTransferType(
                    request.TransferType
                );

            var target =
                await ValidateReservationTargetAsync(
                    request.SlotId,
                    request.EnergyAmountKwh,
                    id
                );

            bool duplicateReservation =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.Id != id &&
                        reservation.ProsumerNic ==
                            prosumerNic &&
                        reservation.SlotId ==
                            request.SlotId &&
                        (
                            reservation.Status ==
                                ReservationStatuses.Pending ||
                            reservation.Status ==
                                ReservationStatuses.Approved
                        ))
                    .AnyAsync();

            if (duplicateReservation)
            {
                throw new InvalidOperationException(
                    "You already have another active reservation for the selected booking slot."
                );
            }

            var update =
                Builders<EnergyReservation>.Update
                    .Set(
                        reservation =>
                            reservation.StationId,
                        target.Slot.StationId
                    )
                    .Set(
                        reservation =>
                            reservation.SlotId,
                        target.Slot.Id!
                    )
                    .Set(
                        reservation =>
                            reservation.EnergyAmountKwh,
                        request.EnergyAmountKwh
                    )
                    .Set(
                        reservation =>
                            reservation.TransferType,
                        transferType
                    )
                    .Set(
                        reservation =>
                            reservation.Status,
                        ReservationStatuses.Pending
                    )
                    .Unset(
                        reservation =>
                            reservation.QrToken
                    )
                    .Set(
                        reservation =>
                            reservation.UpdatedAtUtc,
                        DateTime.UtcNow
                    );

            await _mongoDbService.Reservations
                .UpdateOneAsync(
                    reservation =>
                        reservation.Id == id,
                    update
                );

            return await GetResponseByIdAsync(id);
        }

        // Cancels a reservation if the actor has permission and 12 hours remain.
        public async Task<ReservationResponse?> CancelAsync(
            string id,
            string actorUserId,
            string actorRole)
        {
            ValidateObjectId(id, "reservation");

            var reservation =
                await GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return null;
            }

            if (actorRole == UserRoles.Prosumer &&
                reservation.ProsumerNic != actorUserId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot cancel another Prosumer's reservation."
                );
            }

            if (actorRole != UserRoles.Prosumer &&
                actorRole != UserRoles.GridOperator &&
                actorRole != UserRoles.Backoffice)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to cancel this reservation."
                );
            }

            if (reservation.Status !=
                    ReservationStatuses.Pending &&
                reservation.Status !=
                    ReservationStatuses.Approved)
            {
                throw new InvalidOperationException(
                    "Only pending or approved reservations can be cancelled."
                );
            }

            var slot =
                await _mongoDbService.BookingSlots
                    .Find(item =>
                        item.Id == reservation.SlotId)
                    .FirstOrDefaultAsync();

            if (slot == null)
            {
                throw new InvalidOperationException(
                    "The reservation's booking slot could not be found."
                );
            }

            EnsureTwelveHourNotice(slot);

            var update =
                Builders<EnergyReservation>.Update
                    .Set(
                        item => item.Status,
                        ReservationStatuses.Cancelled
                    )
                    .Unset(
                        item => item.QrToken
                    )
                    .Set(
                        item => item.UpdatedAtUtc,
                        DateTime.UtcNow
                    );

            await _mongoDbService.Reservations
                .UpdateOneAsync(
                    item => item.Id == id,
                    update
                );

            return await GetResponseByIdAsync(id);
        }

        // Approves a pending reservation and generates its secure QR transaction token.
        public async Task<ReservationResponse?> ApproveAsync(
            string id)
        {
            ValidateObjectId(id, "reservation");

            var reservation =
                await GetReservationByIdAsync(id);

            if (reservation == null)
            {
                return null;
            }

            if (reservation.Status !=
                ReservationStatuses.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending reservations can be approved."
                );
            }

            var slot =
                await _mongoDbService.BookingSlots
                    .Find(item =>
                        item.Id == reservation.SlotId)
                    .FirstOrDefaultAsync();

            if (slot == null ||
                !slot.IsActive)
            {
                throw new InvalidOperationException(
                    "The reservation cannot be approved because its booking slot is inactive or missing."
                );
            }

            var station =
                await _mongoDbService.Stations
                    .Find(item =>
                        item.Id == reservation.StationId)
                    .FirstOrDefaultAsync();

            if (station == null ||
                !station.IsActive)
            {
                throw new InvalidOperationException(
                    "The reservation cannot be approved because its solar station is inactive or missing."
                );
            }

            if (slot.StartTimeUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Past reservations cannot be approved."
                );
            }

            string qrToken = GenerateSecureQrToken();

            var update =
                Builders<EnergyReservation>.Update
                    .Set(
                        item => item.Status,
                        ReservationStatuses.Approved
                    )
                    .Set(
                        item => item.QrToken,
                        qrToken
                    )
                    .Set(
                        item => item.UpdatedAtUtc,
                        DateTime.UtcNow
                    );

            await _mongoDbService.Reservations
                .UpdateOneAsync(
                    item => item.Id == id,
                    update
                );

            return await GetResponseByIdAsync(id);
        }

        // Validates the selected slot, station, seven-day rule and reservation capacity.
        private async Task<(
            EnergyBookingSlot Slot,
            SolarStation Station)>
            ValidateReservationTargetAsync(
                string slotId,
                double energyAmountKwh,
                string? excludedReservationId)
        {
            ValidateObjectId(slotId, "booking slot");

            var slot =
                await _mongoDbService.BookingSlots
                    .Find(item => item.Id == slotId)
                    .FirstOrDefaultAsync();

            if (slot == null)
            {
                throw new InvalidOperationException(
                    "Booking slot not found."
                );
            }

            if (!slot.IsActive)
            {
                throw new InvalidOperationException(
                    "The selected booking slot is inactive."
                );
            }

            DateTime nowUtc = DateTime.UtcNow;

            if (slot.StartTimeUtc <= nowUtc)
            {
                throw new InvalidOperationException(
                    "The selected booking slot has already started or passed."
                );
            }

            if (slot.StartTimeUtc >
                nowUtc.AddDays(7))
            {
                throw new InvalidOperationException(
                    "Reservations must be scheduled within the next 7 days."
                );
            }

            var station =
                await _mongoDbService.Stations
                    .Find(item =>
                        item.Id == slot.StationId)
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
                    "Reservations cannot be created for a deactivated station."
                );
            }

            if (energyAmountKwh >
                station.BatteryCapacityKwh)
            {
                throw new InvalidOperationException(
                    $"Energy amount cannot exceed the station battery capacity of {station.BatteryCapacityKwh} kWh."
                );
            }

            var activeFilter =
                Builders<EnergyReservation>.Filter.Eq(
                    reservation =>
                        reservation.SlotId,
                    slotId
                )
                &
                Builders<EnergyReservation>.Filter.In(
                    reservation =>
                        reservation.Status,
                    new[]
                    {
                        ReservationStatuses.Pending,
                        ReservationStatuses.Approved
                    }
                );

            if (!string.IsNullOrWhiteSpace(
                    excludedReservationId))
            {
                activeFilter &=
                    Builders<EnergyReservation>.Filter.Ne(
                        reservation =>
                            reservation.Id,
                        excludedReservationId
                    );
            }

            long activeReservationCount =
                await _mongoDbService.Reservations
                    .CountDocumentsAsync(activeFilter);

            int effectiveCapacity =
                Math.Min(
                    slot.CapacitySlots,
                    station.AvailableBatterySlots
                );

            if (effectiveCapacity <= 0)
            {
                throw new InvalidOperationException(
                    "The station currently has no available battery slots."
                );
            }

            if (activeReservationCount >=
                effectiveCapacity)
            {
                throw new InvalidOperationException(
                    "The selected booking slot is fully reserved."
                );
            }

            return (slot, station);
        }

        // Ensures the reservation still has the required 12-hour modification notice.
        private static void EnsureTwelveHourNotice(
            EnergyBookingSlot slot)
        {
            TimeSpan remaining =
                slot.StartTimeUtc - DateTime.UtcNow;

            if (remaining <
                TimeSpan.FromHours(12))
            {
                throw new InvalidOperationException(
                    "Reservations can only be modified or cancelled when at least 12 hours remain before the booking."
                );
            }
        }

        // Ensures the authenticated account is still an active Prosumer.
        private async Task EnsureActiveProsumerAsync(
            string prosumerNic)
        {
            var user =
                await _mongoDbService.Users
                    .Find(item =>
                        item.Id == prosumerNic &&
                        item.Role ==
                            UserRoles.Prosumer &&
                        item.Status ==
                            UserStatuses.Active)
                    .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "An active Prosumer account is required."
                );
            }
        }

        // Normalizes and validates the requested energy transfer type.
        private static string NormalizeTransferType(
            string transferType)
        {
            string normalized =
                transferType
                    .Trim()
                    .ToUpperInvariant();

            if (normalized !=
                    ReservationTransferTypes.DropOff &&
                normalized !=
                    ReservationTransferTypes.Charging)
            {
                throw new InvalidOperationException(
                    "Transfer type must be DROP_OFF or CHARGING."
                );
            }

            return normalized;
        }

        // Generates a cryptographically secure token for an approved reservation.
        private static string GenerateSecureQrToken()
        {
            byte[] tokenBytes =
                RandomNumberGenerator.GetBytes(32);

            return Convert.ToHexString(tokenBytes);
        }

        // Converts a reservation document into client-friendly response data.
        private async Task<ReservationResponse>
            BuildResponseAsync(
                EnergyReservation reservation)
        {
            var station =
                await _mongoDbService.Stations
                    .Find(item =>
                        item.Id ==
                        reservation.StationId)
                    .FirstOrDefaultAsync();

            var slot =
                await _mongoDbService.BookingSlots
                    .Find(item =>
                        item.Id ==
                        reservation.SlotId)
                    .FirstOrDefaultAsync();

            return new ReservationResponse
            {
                Id = reservation.Id ?? string.Empty,

                ProsumerNic =
                    reservation.ProsumerNic,

                StationId =
                    reservation.StationId,

                StationName =
                    station?.Name ?? "Unknown Station",

                SlotId =
                    reservation.SlotId,

                StartTimeUtc =
                    slot?.StartTimeUtc ??
                    DateTime.MinValue,

                EndTimeUtc =
                    slot?.EndTimeUtc ??
                    DateTime.MinValue,

                EnergyAmountKwh =
                    reservation.EnergyAmountKwh,

                TransferType =
                    reservation.TransferType,

                Status =
                    reservation.Status,

                QrToken =
                    reservation.QrToken,

                CreatedAtUtc =
                    reservation.CreatedAtUtc,

                UpdatedAtUtc =
                    reservation.UpdatedAtUtc,

                CompletedAtUtc =
                    reservation.CompletedAtUtc
            };
        }

        // Verifies that a supplied value is a valid MongoDB ObjectId.
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

        // Returns future pending and approved reservations belonging to a Prosumer.
        public async Task<List<ReservationResponse>>
            GetCurrentReservationsAsync(string prosumerNic)
        {
            var allReservations =
                await GetMyReservationsAsync(prosumerNic);

            DateTime nowUtc = DateTime.UtcNow;

            return allReservations
                .Where(reservation =>
                    reservation.EndTimeUtc > nowUtc &&
                    (
                        reservation.Status ==
                            ReservationStatuses.Pending ||
                        reservation.Status ==
                            ReservationStatuses.Approved
                    ))
                .OrderBy(reservation =>
                    reservation.StartTimeUtc)
                .ToList();
        }

        // Returns completed, cancelled and past reservations for booking history.
        public async Task<List<ReservationResponse>>
            GetBookingHistoryAsync(string prosumerNic)
        {
            var allReservations =
                await GetMyReservationsAsync(prosumerNic);

            DateTime nowUtc = DateTime.UtcNow;

            return allReservations
                .Where(reservation =>
                    reservation.Status ==
                        ReservationStatuses.Completed ||

                    reservation.Status ==
                        ReservationStatuses.Cancelled ||

                    reservation.EndTimeUtc < nowUtc)
                .OrderByDescending(reservation =>
                    reservation.StartTimeUtc)
                .ToList();
        }

        // Searches and filters bookings belonging to a Prosumer.
        public async Task<List<ReservationResponse>>
            SearchMyReservationsAsync(
                string prosumerNic,
                string? status,
                string? search,
                DateTimeOffset? fromDate,
                DateTimeOffset? toDate)
        {
            var reservations =
                await GetMyReservationsAsync(prosumerNic);

            IEnumerable<ReservationResponse> result =
                reservations;

            if (!string.IsNullOrWhiteSpace(status))
            {
                string normalizedStatus =
                    status.Trim().ToUpperInvariant();

                string[] validStatuses =
                {
            ReservationStatuses.Pending,
            ReservationStatuses.Approved,
            ReservationStatuses.Cancelled,
            ReservationStatuses.Completed
        };

                if (!validStatuses.Contains(normalizedStatus))
                {
                    throw new InvalidOperationException(
                        "Invalid reservation status filter."
                    );
                }

                result = result.Where(reservation =>
                    reservation.Status == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string normalizedSearch =
                    search.Trim();

                result = result.Where(reservation =>
                    reservation.StationName.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    reservation.TransferType.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    reservation.Status.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    ));
            }

            if (fromDate.HasValue)
            {
                DateTime fromUtc =
                    fromDate.Value.UtcDateTime;

                result = result.Where(reservation =>
                    reservation.StartTimeUtc >= fromUtc);
            }

            if (toDate.HasValue)
            {
                DateTime toUtc =
                    toDate.Value.UtcDateTime;

                result = result.Where(reservation =>
                    reservation.StartTimeUtc <= toUtc);
            }

            return result
                .OrderByDescending(reservation =>
                    reservation.StartTimeUtc)
                .ToList();
        }

        // Calculates booking statistics for the Prosumer dashboard.
        public async Task<ProsumerDashboardResponse>
            GetProsumerDashboardAsync(string prosumerNic)
        {
            var reservations =
                await GetMyReservationsAsync(prosumerNic);

            DateTime nowUtc = DateTime.UtcNow;

            return new ProsumerDashboardResponse
            {
                CurrentReservations =
                    reservations.Count(reservation =>
                        reservation.EndTimeUtc > nowUtc &&
                        (
                            reservation.Status ==
                                ReservationStatuses.Pending ||
                            reservation.Status ==
                                ReservationStatuses.Approved
                        )),

                PendingReservations =
                    reservations.Count(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Pending &&
                        reservation.StartTimeUtc > nowUtc),

                ApprovedFutureReservations =
                    reservations.Count(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Approved &&
                        reservation.StartTimeUtc > nowUtc),

                CompletedReservations =
                    reservations.Count(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Completed),

                CancelledReservations =
                    reservations.Count(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Cancelled)
            };
        }

        // Calculates operational statistics for Backoffice and Grid Operator dashboards.
        public async Task<StaffDashboardResponse>
            GetStaffDashboardAsync()
        {
            DateTime nowUtc = DateTime.UtcNow;

            int pending =
                (int)await _mongoDbService.Reservations
                    .CountDocumentsAsync(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Pending);

            var approvedReservations =
                await _mongoDbService.Reservations
                    .Find(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Approved)
                    .ToListAsync();

            int approvedFuture = 0;

            foreach (var reservation in approvedReservations)
            {
                var slot =
                    await _mongoDbService.BookingSlots
                        .Find(item =>
                            item.Id ==
                            reservation.SlotId)
                        .FirstOrDefaultAsync();

                if (slot != null &&
                    slot.StartTimeUtc > nowUtc)
                {
                    approvedFuture++;
                }
            }

            int completed =
                (int)await _mongoDbService.Reservations
                    .CountDocumentsAsync(reservation =>
                        reservation.Status ==
                            ReservationStatuses.Completed);

            int activeStations =
                (int)await _mongoDbService.Stations
                    .CountDocumentsAsync(station =>
                        station.IsActive);

            return new StaffDashboardResponse
            {
                PendingReservations = pending,
                ApprovedFutureReservations =
                    approvedFuture,
                CompletedReservations = completed,
                ActiveStations = activeStations
            };
        }
    }
}