/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: User.cs
 * Description: Represents a system user stored in MongoDB.
 */

using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolar.API.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        [BsonIgnoreIfNull]
        public string? Nic { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}