/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: BookingSlotsController.cs
 * Description: Provides energy booking-slot management endpoints.
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
    public class BookingSlotsController : ControllerBase
    {
        private readonly BookingSlotService _bookingSlotService;

        // Initializes the controller with booking-slot services.
        public BookingSlotsController(
            BookingSlotService bookingSlotService)
        {
            _bookingSlotService = bookingSlotService;
        }

        // Returns active future slots for a selected station.
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetAvailableSlots(
            string stationId)
        {
            try
            {
                var slots = await _bookingSlotService
                    .GetAvailableSlotsForStationAsync(stationId);

                return Ok(slots);
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
                return NotFound(new
                {
                    message = exception.Message
                });
            }
        }

        // Returns all active and inactive slots for staff management.
        [HttpGet("manage/station/{stationId}")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> GetAllSlots(
            string stationId)
        {
            try
            {
                var slots = await _bookingSlotService
                    .GetAllSlotsForStationAsync(stationId);

                return Ok(slots);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Returns one booking slot by its unique identifier.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSlot(string id)
        {
            try
            {
                var slot =
                    await _bookingSlotService.GetByIdAsync(id);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        message = "Booking slot not found."
                    });
                }

                return Ok(slot);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Creates a booking slot for a solar station.
        [HttpPost]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> CreateSlot(
            CreateBookingSlotRequest request)
        {
            try
            {
                var slot = await _bookingSlotService
                    .CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetSlot),
                    new
                    {
                        id = slot.Id
                    },
                    slot
                );
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

        // Updates an existing unused booking slot.
        [HttpPut("{id}")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> UpdateSlot(
            string id,
            UpdateBookingSlotRequest request)
        {
            try
            {
                var slot = await _bookingSlotService
                    .UpdateAsync(id, request);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        message = "Booking slot not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Booking slot updated successfully.",

                    slot
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

        // Deactivates an existing booking slot.
        [HttpPatch("{id}/deactivate")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> DeactivateSlot(
            string id)
        {
            try
            {
                bool updated = await _bookingSlotService
                    .DeactivateAsync(id);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Booking slot not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Booking slot deactivated successfully."
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

        // Reactivates a previously deactivated booking slot.
        [HttpPatch("{id}/activate")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> ActivateSlot(
            string id)
        {
            try
            {
                bool updated = await _bookingSlotService
                    .ActivateAsync(id);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Booking slot not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Booking slot activated successfully."
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

        // Permanently deletes an unused booking slot.
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> DeleteSlot(
            string id)
        {
            try
            {
                bool deleted = await _bookingSlotService
                    .DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message = "Booking slot not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Booking slot deleted successfully."
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
    }
}