/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: QrTokenRequest.cs
 * Description: Represents a QR transaction token submitted by a Grid Operator.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class QrTokenRequest
    {
        [Required]
        public string QrToken { get; set; } = string.Empty;
    }
}