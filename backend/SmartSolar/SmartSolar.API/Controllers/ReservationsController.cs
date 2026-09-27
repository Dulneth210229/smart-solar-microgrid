/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ReservationsController.cs
 * Description: Provides Prosumer reservation and staff approval endpoints.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.API.DTOs;
using SmartSolar.API.Helpers;
using SmartSolar.API.Services;
using System.Security.Claims;

namespace SmartSolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        // Initializes the controller with reservation services.
        public ReservationsController(
            ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // Returns all reservations belonging to the authenticated Prosumer.
        [HttpGet("my")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> GetMyReservations()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var reservations =
                await _reservationService
                    .GetMyReservationsAsync(userId);

            return Ok(reservations);
        }

        // Returns all reservations waiting for staff approval.
        [HttpGet("pending")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult>
            GetPendingReservations()
        {
            var reservations =
                await _reservationService
                    .GetPendingReservationsAsync();

            return Ok(reservations);
        }

        // Returns one reservation while enforcing ownership for Prosumers.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(
            string id)
        {
            try
            {
                var reservation =
                    await _reservationService
                        .GetReservationByIdAsync(id);

                if (reservation == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Reservation not found."
                    });
                }

                if (User.IsInRole(UserRoles.Prosumer))
                {
                    string? userId =
                        User.FindFirstValue(
                            ClaimTypes.NameIdentifier
                        );

                    if (reservation.ProsumerNic !=
                        userId)
                    {
                        return Forbid();
                    }
                }

                var response =
                    await _reservationService
                        .GetResponseByIdAsync(id);

                return Ok(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }

        // Creates a new pending reservation for the authenticated Prosumer.
        [HttpPost]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> CreateReservation(
            CreateReservationRequest request)
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                var reservation =
                    await _reservationService.CreateAsync(
                        userId,
                        request
                    );

                return CreatedAtAction(
                    nameof(GetReservation),

                    new
                    {
                        id = reservation.Id
                    },

                    reservation
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
            catch (UnauthorizedAccessException exception)
            {
                return Unauthorized(new
                {
                    message = exception.Message
                });
            }
        }

        // Updates an existing Prosumer reservation.
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> UpdateReservation(
            string id,
            UpdateReservationRequest request)
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                var reservation =
                    await _reservationService.UpdateAsync(
                        userId,
                        id,
                        request
                    );

                if (reservation == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Reservation not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Reservation updated successfully. Approval is required for the updated reservation.",

                    reservation
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
            catch (UnauthorizedAccessException exception)
            {
                return Unauthorized(new
                {
                    message = exception.Message
                });
            }
        }

        // Cancels a reservation through the Prosumer or authorized staff.
        [HttpPatch("{id}/cancel")]
        [Authorize(
            Roles =
                UserRoles.Prosumer + "," +
                UserRoles.GridOperator + "," +
                UserRoles.Backoffice
        )]
        public async Task<IActionResult> CancelReservation(
            string id)
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            string? role = User.FindFirstValue(
                ClaimTypes.Role
            );

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(role))
            {
                return Unauthorized();
            }

            try
            {
                var reservation =
                    await _reservationService.CancelAsync(
                        id,
                        userId,
                        role
                    );

                if (reservation == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Reservation not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Reservation cancelled successfully.",

                    reservation
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
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Approves a pending reservation and creates its secure QR token.
        [HttpPatch("{id}/approve")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult> ApproveReservation(
            string id)
        {
            try
            {
                var reservation =
                    await _reservationService
                        .ApproveAsync(id);

                if (reservation == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Reservation not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Reservation approved successfully. Secure QR token generated.",

                    reservation
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
        // Returns the authenticated Prosumer's current bookings.
        [HttpGet("my/current")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> GetCurrentBookings()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var bookings =
                await _reservationService
                    .GetCurrentReservationsAsync(userId);

            return Ok(bookings);
        }

        // Returns booking history for the authenticated Prosumer.
        [HttpGet("my/history")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> GetBookingHistory()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var bookings =
                await _reservationService
                    .GetBookingHistoryAsync(userId);

            return Ok(bookings);
        }

        // Searches and filters the authenticated Prosumer's bookings.
        [HttpGet("my/search")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> SearchBookings(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                var bookings =
                    await _reservationService
                        .SearchMyReservationsAsync(
                            userId,
                            status,
                            search,
                            fromDate,
                            toDate
                        );

                return Ok(bookings);
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