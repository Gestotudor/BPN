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

[ApiController]
[Route("api/transfers")]
public sealed class TransfersController : ControllerBase
{
    private readonly ISender _sender;

    public TransfersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
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

    [HttpGet("{id:guid}")]
    [RequiredScopes("transfer.read")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransferByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("code/{transactionCode}")]
    [RequiredScopes("transfer.read")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCodeAsync(string transactionCode, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTransferByCodeQuery(transactionCode), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("receive")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveAsync(
        [FromBody] ReceiveTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReceiveTransferCommand(request.TransactionCode, request.ReceiverCustomerId),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelAsync(
        Guid id,
        [FromBody] CancelTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelTransferCommand(id, request.Reason), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("/api/internal/customer-status-changed")]
    [RequiredScopes("transfer.write")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
