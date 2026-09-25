/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: SolarStation.cs
 * Description: Represents a solar microgrid station stored in MongoDB.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.API.Models
{
    public class SolarStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double CapacityKw { get; set; }

        public double BatteryCapacityKwh { get; set; }

        public int TotalBatterySlots { get; set; }

        public int AvailableBatterySlots { get; set; }

        public string OpeningTime { get; set; } = "08:00";

        public string ClosingTime { get; set; } = "18:00";

        public List<string> OperatingDays { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}