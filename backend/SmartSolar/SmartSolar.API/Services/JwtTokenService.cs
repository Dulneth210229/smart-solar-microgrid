/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: JwtTokenService.cs
 * Description: Generates JWT authentication tokens for authenticated users.
 */

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartSolar.API.Models;
using SmartSolar.API.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartSolar.API.Services
{
    public class JwtTokenService
    {
        private readonly JwtSettings _settings;

        // Loads JWT configuration values used when tokens are generated.
        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        // Generates a signed JWT containing the user's ID, role and identity information.
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (!string.IsNullOrWhiteSpace(user.Nic))
            {
                claims.Add(new Claim("nic", user.Nic));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.SecretKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _settings.ExpirationMinutes
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}