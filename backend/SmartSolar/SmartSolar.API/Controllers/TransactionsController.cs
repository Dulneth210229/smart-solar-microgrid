/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: TransactionsController.cs
 * Description: Provides Grid Operator QR verification and energy-transfer completion endpoints.
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
    [Authorize(Roles = UserRoles.GridOperator)]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        // Initializes the controller with transaction services.
        public TransactionsController(
            TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // Verifies a QR token against the approved reservation stored on the server.
        [HttpPost("verify-qr")]
        public async Task<IActionResult> VerifyQr(
            QrTokenRequest request)
        {
            try
            {
                var result =
                    await _transactionService
                        .VerifyQrAsync(request.QrToken);

                return Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    valid = false,
                    message = exception.Message
                });
            }
        }

        // Finalizes the energy-transfer job after QR verification.
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteTransfer(
            QrTokenRequest request)
        {
            try
            {
                var reservation =
                    await _transactionService
                        .CompleteTransferAsync(
                            request.QrToken
                        );

                return Ok(new
                {
                    message =
                        "Energy transfer completed successfully.",

                    reservation
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