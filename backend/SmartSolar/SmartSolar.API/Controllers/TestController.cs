/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: TestController.cs
 * Description: Provides endpoints used to verify API and MongoDB connectivity.
 */

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SmartSolar.API.Services;

namespace SmartSolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        // Initializes the controller with the MongoDB service.
        public TestController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Checks whether the ASP.NET Core Web API is running.
        [HttpGet]
        public IActionResult GetApiStatus()
        {
            return Ok(new
            {
                message = "Smart Solar API is running successfully.",
                timestamp = DateTime.UtcNow
            });
        }

        // Checks whether the Web API can communicate with MongoDB.
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                bool connected =
                    await _mongoDbService.CheckConnectionAsync();

                return Ok(new
                {
                    connected = connected,
                    message = "MongoDB connection successful."
                });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new
                {
                    connected = false,
                    message = "MongoDB connection failed.",
                    error = exception.Message
                });
            }
        }

        // Retrieves MongoDB collection names and verifies the required collections.
        [HttpGet("collections")]
        public async Task<IActionResult> GetDatabaseCollections()
        {
            try
            {
                var database = _mongoDbService.GetDatabase();

                var collectionNames = await database
                    .ListCollectionNames()
                    .ToListAsync();

                string[] requiredCollections =
                {
                    "UserDetails",
                    "SolarStationInfo",
                    "EnergyBookingSlots",
                    "EnergyReservation"
                };

                bool allCollectionsExist =
                    requiredCollections.All(
                        name => collectionNames.Contains(name)
                    );

                return Ok(new
                {
                    databaseName =
                        database.DatabaseNamespace.DatabaseName,

                    allCollectionsExist = allCollectionsExist,

                    collections = collectionNames
                });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new
                {
                    message =
                        "Failed to retrieve MongoDB collections.",

                    error = exception.Message
                });
            }
        }
    }
}