using BuildingBlocks.Middleware;
using BuildingBlocks.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransferService.API.Extensions;
using TransferService.Application.Features.Transfers.Commands.CancelTransfer;
using TransferService.Application.Features.Transfers.Commands.CreateTransfer;
using TransferService.Application.Features.Transfers.Commands.HandleCustomerStatusChanged;
using TransferService.Application.Features.Transfers.Commands.ReceiveTransfer;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Features.Transfers.Queries.GetTransferByCode;
using TransferService.Application.Features.Transfers.Queries.GetTransferById;

namespace TransferService.API.Controllers;

/// <summary>
/// Manages transfer creation, receipt, cancellation, and internal transfer notifications.
/// </summary>
[ApiController]
[Route("api/transfers")]
[Produces("application/json")]
public sealed class TransfersController : ControllerBase
{
    private readonly ISender _sender;

    public TransfersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new money transfer.
    /// </summary>
    /// <remarks>
    /// Creates a money transfer after validating sender and receiver customers, applying exchange rate if needed, checking the daily limit, calculating the fee, and calling the fraud detection service. Every transfer requires a fraud check.
    /// Headers: <c>X-API-Key</c>, <c>Idempotency-Key</c>.
    /// </remarks>
    [HttpPost]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(ApiResponse<object>.Fail("Idempotency-Key header is required."));
        }

        var result = await _sender.Send(
            new CreateTransferCommand(
                request.SenderCustomer,
                request.ReceiverCustomer,
                request.Amount,
                request.Currency,
                idempotencyKey.ToString()),
            cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Gets a transfer by identifier.
    /// </summary>
    /// <param name="id">Transfer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [RequiredScopes("transfer.read")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransferByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Gets a transfer by transaction code.
    /// </summary>
    /// <param name="transactionCode">MoneyBee transaction code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("code/{transactionCode}")]
    [RequiredScopes("transfer.read")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCodeAsync(string transactionCode, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransferByCodeQuery(transactionCode), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Completes a pending transfer using a transaction code.
    /// </summary>
    /// <remarks>
    /// The receiver provides the transaction code. A transfer can be completed only if it is <c>Pending</c> and the 5-minute waiting period has passed for transfers above 1000 TRY.
    /// </remarks>
    [HttpPost("receive")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ReceiveAsync(
        [FromBody] ReceiveTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReceiveTransferCommand(request.TransactionCode, request.ReceiverCustomerId),
            cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Cancels a pending transfer.
    /// </summary>
    /// <param name="id">Transfer identifier.</param>
    /// <param name="request">Cancellation reason payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Cancels a transfer if the money has not been received yet and creates a fee refund record.
    /// </remarks>
    [HttpPost("{id:guid}/cancel")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelAsync(
        Guid id,
        [FromBody] CancelTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelTransferCommand(id, request.Reason), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Handles a customer status change notification.
    /// </summary>
    /// <remarks>
    /// Called by the Customer service. If a customer becomes <c>Blocked</c>, all related pending transfers are cancelled automatically and fees are refunded.
    /// </remarks>
    [HttpPost("/api/internal/customer-status-changed")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CustomerStatusChangedAsync(
        [FromBody] CustomerStatusChangedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new HandleCustomerStatusChangedCommand(request.CustomerId, request.OldStatus, request.NewStatus),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return Ok(ApiResponse<object>.Ok(new { CancelledPendingTransferCount = result.Value }));
    }
}
