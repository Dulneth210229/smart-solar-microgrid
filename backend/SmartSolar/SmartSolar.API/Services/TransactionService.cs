/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: TransactionService.cs
 * Description: Verifies QR transaction tokens and finalizes approved energy transfers.
 */

using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class TransactionService
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes transaction database access.
        public TransactionService(
            MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Verifies that a scanned QR token belongs to a valid approved reservation.
        public async Task<QrVerificationResponse>
            VerifyQrAsync(string qrToken)
        {
            string token = qrToken.Trim();

            var reservation =
                await _mongoDbService.Reservations
                    .Find(item =>
                        item.QrToken == token)
                    .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new InvalidOperationException(
                    "QR transaction token is invalid."
                );
            }

            if (reservation.Status !=
                ReservationStatuses.Approved)
            {
                throw new InvalidOperationException(
                    $"Reservation cannot be verified because its current status is {reservation.Status}."
                );
            }

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

            if (station == null || slot == null)
            {
                throw new InvalidOperationException(
                    "Reservation references missing station or booking-slot data."
                );
            }

            return new QrVerificationResponse
            {
                Valid = true,

                ReservationId =
                    reservation.Id ?? string.Empty,

                ProsumerNic =
                    reservation.ProsumerNic,

                StationName =
                    station.Name,

                StartTimeUtc =
                    slot.StartTimeUtc,

                EndTimeUtc =
                    slot.EndTimeUtc,

                EnergyAmountKwh =
                    reservation.EnergyAmountKwh,

                TransferType =
                    reservation.TransferType,

                Status =
                    reservation.Status
            };
        }

        // Marks a verified approved reservation as completed.
        public async Task<ReservationResponse>
            CompleteTransferAsync(string qrToken)
        {
            string token = qrToken.Trim();

            var reservation =
                await _mongoDbService.Reservations
                    .Find(item =>
                        item.QrToken == token)
                    .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new InvalidOperationException(
                    "QR transaction token is invalid."
                );
            }

            if (reservation.Status !=
                ReservationStatuses.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved reservations can be completed."
                );
            }

            DateTime completedAtUtc =
                DateTime.UtcNow;

            var update =
                Builders<EnergyReservation>.Update
                    .Set(
                        item => item.Status,
                        ReservationStatuses.Completed
                    )
                    .Set(
                        item => item.CompletedAtUtc,
                        completedAtUtc
                    )
                    .Set(
                        item => item.UpdatedAtUtc,
                        completedAtUtc
                    );

            await _mongoDbService.Reservations
                .UpdateOneAsync(
                    item =>
                        item.Id == reservation.Id,
                    update
                );

            var updated =
                await _mongoDbService.Reservations
                    .Find(item =>
                        item.Id == reservation.Id)
                    .FirstOrDefaultAsync();

            return await BuildResponseAsync(updated);
        }

        // Builds complete reservation information for the transaction result.
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
                Id =
                    reservation.Id ?? string.Empty,

                ProsumerNic =
                    reservation.ProsumerNic,

                StationId =
                    reservation.StationId,

                StationName =
                    station?.Name ??
                    "Unknown Station",

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
    }
}