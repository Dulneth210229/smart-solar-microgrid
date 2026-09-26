/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UpdateBookingSlotRequest.cs
 * Description: Represents editable information for an existing booking slot.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class UpdateBookingSlotRequest
    {
        [Required]
        public DateTimeOffset StartTime { get; set; }

        [Required]
        public DateTimeOffset EndTime { get; set; }

        [Range(1, int.MaxValue)]
        public int CapacitySlots { get; set; }
    }
}