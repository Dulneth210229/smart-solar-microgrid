/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: RegisterProsumerRequest.cs
 * Description: Represents data submitted when a Prosumer creates an account.
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.API.DTOs
{
    public class RegisterProsumerRequest
    {
        [Required]
        public string Nic { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}