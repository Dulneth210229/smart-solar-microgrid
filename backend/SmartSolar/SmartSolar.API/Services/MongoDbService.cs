/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: MongoDbService.cs
 * Description: Creates and maintains the connection between the Web API and MongoDB.
 */

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.API.Settings;

namespace SmartSolar.API.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        // Initializes the MongoDB client using configuration from appsettings.json.
        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            MongoClient client = new MongoClient(settings.Value.ConnectionString);

            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        // Checks whether the API can successfully communicate with MongoDB.
        public async Task<bool> CheckConnectionAsync()
        {
            await _database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1)
            );

            return true;
        }

        // Provides access to the configured MongoDB database for application services.
        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}