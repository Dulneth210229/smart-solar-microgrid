/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: CreateReservationRequest.cs
 * Description: Represents data submitted when a Prosumer creates an energy reservation.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class CreateReservationRequest
    {
        [Required]
        public string SlotId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double EnergyAmountKwh { get; set; }

        [Required]
        public string TransferType { get; set; } = string.Empty;
    }
}