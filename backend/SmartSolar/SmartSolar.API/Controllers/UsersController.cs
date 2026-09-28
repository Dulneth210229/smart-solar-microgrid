/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UsersController.cs
 * Description: Provides protected user and account-management endpoints.
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
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        // Initializes the controller with user-management services.
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // Returns the profile of the currently authenticated user.
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.Nic,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.Status,
                user.CreatedAtUtc
            });
        }

        // Updates profile information for the authenticated user.
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateProfileRequest request)
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
                bool updated = await _userService
                    .UpdateProfileAsync(userId, request);

                if (!updated)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    message = "Profile updated successfully."
                });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new
                {
                    message = exception.Message
                });
            }
        }

        // Allows a Prosumer to request deactivation of their own account.
        [HttpPatch("me/request-deactivation")]
        [Authorize(Roles = UserRoles.Prosumer)]
        public async Task<IActionResult> RequestDeactivation()
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool updated = await _userService
                .RequestDeactivationAsync(userId);

            if (!updated)
            {
                return BadRequest(new
                {
                    message =
                        "Account deactivation could not be requested."
                });
            }

            return Ok(new
            {
                message =
                    "Account deactivation request submitted."
            });
        }

        // Returns all Prosumer accounts waiting for activation.
        [HttpGet("pending-prosumers")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> GetPendingProsumers()
        {
            var users = await _userService
                .GetPendingProsumersAsync();

            return Ok(users.Select(user => new
            {
                user.Id,
                user.Nic,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Status,
                user.CreatedAtUtc
            }));
        }

        // Activates or reactivates a user account.
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> ActivateUser(string id)
        {
            bool updated = await _userService.ActivateAsync(id);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "User account activated successfully."
            });
        }

        // Deactivates a user account after Backoffice confirmation.
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            bool updated = await _userService.DeactivateAsync(id);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "User account deactivated successfully."
            });
        }

        // Creates a new Backoffice or Grid Operator account.
        [HttpPost("staff")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> CreateStaffUser(
            CreateStaffUserRequest request)
        {
            try
            {
                var user = await _userService
                    .CreateStaffUserAsync(request);

                return StatusCode(201, new
                {
                    message = "Staff account created successfully.",
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Status
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

        // Returns all system users for Backoffice administration.
        [HttpGet]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(users.Select(user => new
            {
                user.Id,
                user.Nic,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.Status,
                user.CreatedAtUtc,
                user.UpdatedAtUtc
            }));
        }

        // Returns Prosumer accounts waiting for deactivation approval.
        [HttpGet("deactivation-requests")]
        [Authorize(Roles = UserRoles.Backoffice)]
        public async Task<IActionResult> GetDeactivationRequests()
        {
            var users =
                await _userService.GetDeactivationRequestsAsync();

            return Ok(users.Select(user => new
            {
                user.Id,
                user.Nic,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Status,
                user.CreatedAtUtc
            }));
        }
    }
}