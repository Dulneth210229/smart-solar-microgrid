/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UserService.cs
 * Description: Handles system-user and Prosumer account management operations.
 */

using MongoDB.Driver;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class UserService
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes user-management database access.
        public UserService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Returns all Prosumer accounts waiting for Backoffice activation.
        public async Task<List<User>> GetPendingProsumersAsync()
        {
            return await _mongoDbService.Users
                .Find(user =>
                    user.Role == UserRoles.Prosumer &&
                    user.Status == UserStatuses.Pending)
                .ToListAsync();
        }

        // Returns a single user using their unique application ID.
        public async Task<User?> GetByIdAsync(string id)
        {
            return await _mongoDbService.Users
                .Find(user => user.Id == id)
                .FirstOrDefaultAsync();
        }

        // Activates a pending or previously deactivated account.
        public async Task<bool> ActivateAsync(string id)
        {
            var result = await _mongoDbService.Users.UpdateOneAsync(
                user => user.Id == id,
                Builders<User>.Update
                    .Set(user => user.Status, UserStatuses.Active)
                    .Set(user => user.UpdatedAtUtc, DateTime.UtcNow)
            );

            return result.MatchedCount > 0;
        }

        // Marks an account as deactivated after Backoffice approval.
        public async Task<bool> DeactivateAsync(string id)
        {
            var result = await _mongoDbService.Users.UpdateOneAsync(
                user => user.Id == id,
                Builders<User>.Update
                    .Set(
                        user => user.Status,
                        UserStatuses.Deactivated
                    )
                    .Set(user => user.UpdatedAtUtc, DateTime.UtcNow)
            );

            return result.MatchedCount > 0;
        }

        // Updates profile information for the currently authenticated user.
        public async Task<bool> UpdateProfileAsync(
            string id,
            UpdateProfileRequest request)
        {
            string normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            var duplicateEmail = await _mongoDbService.Users
                .Find(user =>
                    user.Email == normalizedEmail &&
                    user.Id != id)
                .FirstOrDefaultAsync();

            if (duplicateEmail != null)
            {
                throw new InvalidOperationException(
                    "Another account already uses this email."
                );
            }

            var update = Builders<User>.Update
                .Set(user => user.FullName, request.FullName.Trim())
                .Set(user => user.Email, normalizedEmail)
                .Set(
                    user => user.PhoneNumber,
                    request.PhoneNumber.Trim()
                )
                .Set(user => user.UpdatedAtUtc, DateTime.UtcNow);

            var result = await _mongoDbService.Users
                .UpdateOneAsync(user => user.Id == id, update);

            return result.MatchedCount > 0;
        }

        // Allows an active Prosumer to request account deactivation.
        public async Task<bool> RequestDeactivationAsync(string id)
        {
            var result = await _mongoDbService.Users.UpdateOneAsync(
                user =>
                    user.Id == id &&
                    user.Role == UserRoles.Prosumer &&
                    user.Status == UserStatuses.Active,

                Builders<User>.Update
                    .Set(
                        user => user.Status,
                        UserStatuses.DeactivationRequested
                    )
                    .Set(user => user.UpdatedAtUtc, DateTime.UtcNow)
            );

            return result.ModifiedCount > 0;
        }

        // Creates a Backoffice or Grid Operator account.
        public async Task<User> CreateStaffUserAsync(
            CreateStaffUserRequest request)
        {
            string role = request.Role
                .Trim()
                .ToUpperInvariant();

            if (role != UserRoles.Backoffice &&
                role != UserRoles.GridOperator)
            {
                throw new InvalidOperationException(
                    "Staff role must be BACKOFFICE or GRID_OPERATOR."
                );
            }

            string email = request.Email
                .Trim()
                .ToLowerInvariant();

            var existing = await _mongoDbService.Users
                .Find(user => user.Email == email)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists."
                );
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                FullName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),

                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                ),

                Role = role,
                Status = UserStatuses.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.Users.InsertOneAsync(user);

            return user;
        }
    }
}