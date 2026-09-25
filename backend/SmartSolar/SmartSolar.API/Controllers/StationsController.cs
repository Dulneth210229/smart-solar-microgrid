/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: StationsController.cs
 * Description: Provides solar microgrid station management endpoints.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Services;

namespace SmartSolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StationsController : ControllerBase
    {
        private readonly SolarStationService _stationService;

        // Initializes the controller with solar station services.
        public StationsController(
            SolarStationService stationService)
        {
            _stationService = stationService;
        }

        // Returns active solar stations for authenticated application users.
        [HttpGet]
        public async Task<IActionResult> GetActiveStations()
        {
            var stations =
                await _stationService.GetActiveStationsAsync();

            return Ok(stations);
        }

        // Returns all stations including inactive ones for Backoffice management.
        [HttpGet("manage")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> GetAllStations()
        {
            var stations =
                await _stationService.GetAllStationsAsync();

            return Ok(stations);
        }

        // Returns one solar station using its unique identifier.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStation(string id)
        {
            try
            {
                var station =
                    await _stationService.GetByIdAsync(id);

                if (station == null)
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                if (!station.IsActive &&
                    !User.IsInRole(UserRoles.Backoffice))
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                return Ok(station);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Creates a new solar station.
        [HttpPost]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> CreateStation(
            CreateSolarStationRequest request)
        {
            try
            {
                var station =
                    await _stationService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetStation),

                    new
                    {
                        id = station.Id
                    },

                    station
                );
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Updates an existing solar station and its operating schedule.
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> UpdateStation(
            string id,
            UpdateSolarStationRequest request)
        {
            try
            {
                var station =
                    await _stationService.UpdateAsync(
                        id,
                        request
                    );

                if (station == null)
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Solar station updated successfully.",

                    station
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Updates operational battery-slot availability.
        [HttpPatch("{id}/availability")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> UpdateAvailability(
            string id,
            UpdateStationAvailabilityRequest request)
        {
            try
            {
                var station =
                    await _stationService
                        .UpdateAvailabilityAsync(
                            id,
                            request.AvailableBatterySlots
                        );

                if (station == null)
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Station availability updated successfully.",

                    station
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Deactivates a station when no active reservations exist.
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> DeactivateStation(
            string id)
        {
            try
            {
                bool updated =
                    await _stationService
                        .DeactivateAsync(id);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Solar station deactivated successfully."
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Reactivates a previously deactivated solar station.
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> ActivateStation(
            string id)
        {
            try
            {
                bool updated =
                    await _stationService.ActivateAsync(id);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Solar station not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Solar station activated successfully."
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }
    }
}