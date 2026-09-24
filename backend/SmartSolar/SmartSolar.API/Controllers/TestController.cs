/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: TestController.cs
 * Description: Provides endpoints used to verify API and database connectivity.
 */

using Microsoft.AspNetCore.Mvc;
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

        // Returns a response proving that the ASP.NET Core API is running.
        [HttpGet]
        public IActionResult GetApiStatus()
        {
            return Ok(new
            {
                message = "Smart Solar API is running successfully.",
                timestamp = DateTime.UtcNow
            });
        }

        // Tests whether the Web API can successfully communicate with MongoDB.
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                bool connected = await _mongoDbService.CheckConnectionAsync();

                return Ok(new
                {
                    connected,
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
    }
}