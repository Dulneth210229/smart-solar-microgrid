/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: CreateSolarStationRequest.cs
 * Description: Represents data required to create a solar microgrid station.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class CreateSolarStationRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0.01, double.MaxValue)]
        public double CapacityKw { get; set; }

        [Range(0.01, double.MaxValue)]
        public double BatteryCapacityKwh { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalBatterySlots { get; set; }

        [Required]
        public string OpeningTime { get; set; } = string.Empty;

        [Required]
        public string ClosingTime { get; set; } = string.Empty;

        [Required]
        public List<string> OperatingDays { get; set; } = new();
    }
}