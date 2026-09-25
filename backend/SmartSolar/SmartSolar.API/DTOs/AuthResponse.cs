/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: AuthResponse.cs
 * Description: Represents authentication data returned after successful login.
 */

namespace SmartSolar.API.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Nic { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}