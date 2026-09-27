/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: DashboardController.cs
 * Description: Provides dashboard statistics for Prosumer and staff applications.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.API.Helpers;
using SmartSolar.API.Services;
using System.Security.Claims;

namespace SmartSolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        // Initializes dashboard services.
        public DashboardController(
            ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // Returns booking counts for the authenticated Prosumer.
        [HttpGet("prosumer")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult>
            GetProsumerDashboard()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var dashboard =
                await _reservationService
                    .GetProsumerDashboardAsync(userId);

            return Ok(dashboard);
        }

        // Returns system-wide operational counts for staff.
        [HttpGet("staff")]
        [Authorize(
            Roles =
                UserRoles.Backoffice + "," +
                UserRoles.GridOperator
        )]
        public async Task<IActionResult>
            GetStaffDashboard()
        {
            var dashboard =
                await _reservationService
                    .GetStaffDashboardAsync();

            return Ok(dashboard);
        }
    }
}