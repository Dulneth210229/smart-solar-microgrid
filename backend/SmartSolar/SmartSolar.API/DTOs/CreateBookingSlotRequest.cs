/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: CreateBookingSlotRequest.cs
 * Description: Represents data required to create an energy booking slot.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class CreateBookingSlotRequest
    {
        [Required]
        public string StationId { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset StartTime { get; set; }

        [Required]
        public DateTimeOffset EndTime { get; set; }

        [Range(1, int.MaxValue)]
        public int CapacitySlots { get; set; }
    }
}