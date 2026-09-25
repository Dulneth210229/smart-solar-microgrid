/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: AuthService.cs
 * Description: Handles Prosumer registration and system-user authentication.
 */

using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class AuthService
    {
        private readonly MongoDbService _mongoDbService;
        private readonly JwtTokenService _jwtTokenService;

        // Initializes authentication services and database access.
        public AuthService(
            MongoDbService mongoDbService,
            JwtTokenService jwtTokenService)
        {
            _mongoDbService = mongoDbService;
            _jwtTokenService = jwtTokenService;
        }

        // Registers a new Prosumer account using NIC as its primary identifier.
        public async Task<User> RegisterProsumerAsync(
            RegisterProsumerRequest request)
        {
            string nic = request.Nic.Trim();
            string email = request.Email.Trim().ToLowerInvariant();

            var existingNic = await _mongoDbService.Users
                .Find(user => user.Nic == nic)
                .FirstOrDefaultAsync();

            if (existingNic != null)
            {
                throw new InvalidOperationException(
                    "A user with this NIC already exists."
                );
            }

            var existingEmail = await _mongoDbService.Users
                .Find(user => user.Email == email)
                .FirstOrDefaultAsync();

            if (existingEmail != null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists."
                );
            }

            var user = new User
            {
                Id = nic,
                Nic = nic,
                FullName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                ),
                Role = UserRoles.Prosumer,
                Status = UserStatuses.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.Users.InsertOneAsync(user);

            return user;
        }

        // Validates login credentials and returns authentication data for active users.
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            string identifier = request.Identifier.Trim();

            var user = await _mongoDbService.Users
                .Find(user =>
                    user.Email == identifier.ToLowerInvariant() ||
                    user.Nic == identifier)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid username or password."
                );
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!passwordValid)
            {
                throw new UnauthorizedAccessException(
                    "Invalid username or password."
                );
            }

            if (user.Status != UserStatuses.Active)
            {
                throw new UnauthorizedAccessException(
                    $"Account is not active. Current status: {user.Status}."
                );
            }

            string token = _jwtTokenService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Nic = user.Nic,
                Role = user.Role,
                Status = user.Status
            };
        }
    }
}