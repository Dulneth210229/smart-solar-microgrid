/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UpdateReservationRequest.cs
 * Description: Represents editable information for an energy reservation.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class UpdateReservationRequest
    {
        [Required]
        public string SlotId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double EnergyAmountKwh { get; set; }

        [Required]
        public string TransferType { get; set; } = string.Empty;
    }
}