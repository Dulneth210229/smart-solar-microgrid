/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UpdateStationAvailabilityRequest.cs
 * Description: Represents an update to the available battery slots of a station.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class UpdateStationAvailabilityRequest
    {
        [Range(0, int.MaxValue)]
        public int AvailableBatterySlots { get; set; }
    }
}