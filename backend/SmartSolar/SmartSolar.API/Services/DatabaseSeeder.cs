/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: DatabaseSeeder.cs
 * Description: Creates initial development data required to operate the system.
 */

using MongoDB.Driver;
using SmartSolar.API.Helpers;
using SmartSolar.API.Models;

namespace SmartSolar.API.Services
{
    public class DatabaseSeeder
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes database seeding support.
        public DatabaseSeeder(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Creates the initial Backoffice account if one does not already exist.
        public async Task SeedAsync()
        {
            const string adminEmail = "admin@smartsolar.lk";

            var existing = await _mongoDbService.Users
                .Find(user => user.Email == adminEmail)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return;
            }

            var admin = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                FullName = "System Administrator",
                Email = adminEmail,
                PhoneNumber = "0700000000",

                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    "Admin@123"
                ),

                Role = UserRoles.Backoffice,
                Status = UserStatuses.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _mongoDbService.Users.InsertOneAsync(admin);
        }
    }
}