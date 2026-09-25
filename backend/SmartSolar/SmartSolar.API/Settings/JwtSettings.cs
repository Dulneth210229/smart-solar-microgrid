/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: JwtSettings.cs
 * Description: Stores configuration used to generate JWT authentication tokens.
 */

namespace SmartSolar.API.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int ExpirationMinutes { get; set; } = 120;
    }
}