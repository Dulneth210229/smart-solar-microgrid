/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ReservationResponse.cs
 * Description: Represents reservation information returned to API clients.
 */

namespace SmartSolar.API.DTOs
{
    public class ReservationResponse
    {
        public string Id { get; set; } = string.Empty;

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationId { get; set; } = string.Empty;

        public string StationName { get; set; } = string.Empty;

        public string SlotId { get; set; } = string.Empty;

        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }

        public double EnergyAmountKwh { get; set; }

        public string TransferType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? QrToken { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }
    }
}