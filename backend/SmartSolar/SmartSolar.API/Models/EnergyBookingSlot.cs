/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: EnergyBookingSlot.cs
 * Description: Represents an energy booking time slot at a solar station.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.API.Models
{
    public class EnergyBookingSlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }

        public int CapacitySlots { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}