/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: QrVerificationResponse.cs
 * Description: Represents verified reservation information returned after scanning a QR token.
 */

namespace SmartSolar.API.DTOs
{
    public class QrVerificationResponse
    {
        public bool Valid { get; set; }

        public string ReservationId { get; set; } = string.Empty;

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationName { get; set; } = string.Empty;

        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }

        public double EnergyAmountKwh { get; set; }

        public string TransferType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}