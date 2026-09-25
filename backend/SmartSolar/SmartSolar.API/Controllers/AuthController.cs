/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: AuthController.cs
 * Description: Provides Prosumer registration and user login endpoints.
 */

using Microsoft.AspNetCore.Mvc;
using SmartSolar.API.DTOs;
using SmartSolar.API.Services;

namespace SmartSolar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        // Initializes the controller with authentication services.
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Registers a new Prosumer account with PENDING activation status.
        [HttpPost("register-prosumer")]
        public async Task<IActionResult> RegisterProsumer(
            RegisterProsumerRequest request)
        {
            try
            {
                var user = await _authService
                    .RegisterProsumerAsync(request);

                return StatusCode(201, new
                {
                    message =
                        "Prosumer registered successfully. Account is awaiting Backoffice activation.",

                    userId = user.Id,
                    nic = user.Nic,
                    email = user.Email,
                    role = user.Role,
                    status = user.Status
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

        // Authenticates an active user and returns a JWT token.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);

                return Ok(response);
            }
            catch (UnauthorizedAccessException exception)
            {
                return Unauthorized(new
                {
                    message = exception.Message
                });
            }
        }
    }
}