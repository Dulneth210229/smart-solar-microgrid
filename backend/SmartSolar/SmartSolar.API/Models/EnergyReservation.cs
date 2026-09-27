/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: EnergyReservation.cs
 * Description: Represents an energy transfer reservation created by a Prosumer.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.API.Models
{
    public class EnergyReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ProsumerNic { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string SlotId { get; set; } = string.Empty;

        public double EnergyAmountKwh { get; set; }

        public string TransferType { get; set; } = string.Empty;

        public string Status { get; set; } = "PENDING";

        [BsonIgnoreIfNull]
        public string? QrToken { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [BsonIgnoreIfNull]
        public DateTime? CompletedAtUtc { get; set; }
    }
}