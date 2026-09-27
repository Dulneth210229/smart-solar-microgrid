/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: MongoDbService.cs
 * Description: Manages the MongoDB connection and provides access to application collections.
 */

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolar.API.Models;
using SmartSolar.API.Settings;

namespace SmartSolar.API.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        // Initializes the MongoDB connection using the configured connection string.
        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            MongoClient client = new MongoClient(
                settings.Value.ConnectionString
            );

            _database = client.GetDatabase(
                settings.Value.DatabaseName
            );
        }

        // Provides access to the system users collection.
        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("UserDetails");


        // Provides access to the solar stations collection.
        public IMongoCollection<SolarStation> Stations =>
            _database.GetCollection<SolarStation>("SolarStationInfo");


        // Provides access to the energy booking slots collection.
        public IMongoCollection<EnergyBookingSlot> BookingSlots =>
            _database.GetCollection<EnergyBookingSlot>("EnergyBookingSlots");


        // Provides access to the energy reservations collection.
        public IMongoCollection<EnergyReservation> Reservations =>
            _database.GetCollection<EnergyReservation>("EnergyReservation");


        // Checks whether the MongoDB server is reachable.
        public async Task<bool> CheckConnectionAsync()
        {
            await _database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1)
            );

            return true;
        }

        // Returns the configured MongoDB database.
        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        // Creates unique database indexes for important user identifiers.
        public async Task CreateIndexesAsync()
        {
            var emailIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions
                {
                    Unique = true
                }
            );

            var nicIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(user => user.Nic),
                new CreateIndexOptions
                {
                    Unique = true,
                    Sparse = true
                }
            );

            await Users.Indexes.CreateManyAsync(
                new[]
                {
            emailIndex,
            nicIndex
                }
            );

            var bookingSlotIndex =
              new CreateIndexModel<EnergyBookingSlot>(
                  Builders<EnergyBookingSlot>
                      .IndexKeys
                      .Ascending(slot => slot.StationId)
                      .Ascending(slot => slot.StartTimeUtc)
              );

            await BookingSlots.Indexes.CreateOneAsync(
                bookingSlotIndex
            );

            var reservationProsumerIndex =
    new CreateIndexModel<EnergyReservation>(
        Builders<EnergyReservation>
            .IndexKeys
            .Ascending(
                reservation =>
                    reservation.ProsumerNic
            )
            .Descending(
                reservation =>
                    reservation.CreatedAtUtc
            )
    );

            var reservationSlotStatusIndex =
                new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>
                        .IndexKeys
                        .Ascending(
                            reservation =>
                                reservation.SlotId
                        )
                        .Ascending(
                            reservation =>
                                reservation.Status
                        )
                );

            var reservationStationStatusIndex =
                new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>
                        .IndexKeys
                        .Ascending(
                            reservation =>
                                reservation.StationId
                        )
                        .Ascending(
                            reservation =>
                                reservation.Status
                        )
                );

            var qrTokenIndex =
                new CreateIndexModel<EnergyReservation>(
                    Builders<EnergyReservation>
                        .IndexKeys
                        .Ascending(
                            reservation =>
                                reservation.QrToken
                        ),

                    new CreateIndexOptions
                    {
                        Unique = true,
                        Sparse = true
                    }
                );

            await Reservations.Indexes.CreateManyAsync(
                new[]
                {
        reservationProsumerIndex,
        reservationSlotStatusIndex,
        reservationStationStatusIndex,
        qrTokenIndex
                }
            );
        }
    }
}