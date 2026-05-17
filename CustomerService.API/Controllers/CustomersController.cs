using BuildingBlocks.Middleware;
using BuildingBlocks.Responses;
using CustomerService.API.Extensions;
using CustomerService.Application.Features.Customers.Commands.ChangeCustomerStatus;
using CustomerService.Application.Features.Customers.Commands.CreateCustomer;
using CustomerService.Application.Features.Customers.Commands.UpdateCustomer;
using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Application.Features.Customers.Queries.GetCustomerById;
using CustomerService.Application.Features.Customers.Queries.GetCustomerByNationalId;
using CustomerService.Application.Features.Customers.Queries.ValidateCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Controllers;

/// <summary>
/// Manages customer registration, lookup, status changes, and transfer eligibility checks.
/// </summary>
[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new customer after KYC verification.
    /// </summary>
    /// <remarks>
    /// Registers a new individual or corporate customer. KYC verification is mandatory. Customers who fail KYC verification are not stored. Customers under 18 are rejected.
    /// </remarks>
    [HttpPost]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Gets a customer by identifier.
    /// </summary>
    /// <param name="id">Customer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Gets a customer by national identity number.
    /// </summary>
    /// <param name="nationalIdNumber">Customer national identity number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("by-national-id/{nationalIdNumber}")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByNationalIdAsync(string nationalIdNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByNationalIdQuery(nationalIdNumber), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Updates mutable customer details.
    /// </summary>
    /// <param name="id">Customer identifier.</param>
    /// <param name="request">Updated customer details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCustomerCommand(
                id,
                request.Name,
                request.Surname,
                request.TaxNumber,
                request.PhoneNumber,
                request.DateOfBirth,
                request.Type),
            cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Changes customer status.
    /// </summary>
    /// <param name="id">Customer identifier.</param>
    /// <param name="request">Requested new status and optional reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Changes customer status and notifies the Transfer service. If the customer becomes <c>Blocked</c>, the Transfer service cancels pending transfers.
    /// </remarks>
    [HttpPatch("{id:guid}/status")]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ChangeStatusAsync(
        Guid id,
        [FromBody] ChangeCustomerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ChangeCustomerStatusCommand(id, request.Status, request.Reason),
            cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Validates whether a customer can participate in a transfer.
    /// </summary>
    /// <remarks>
    /// Used by the Transfer service. Returns valid only if the customer exists, is <c>Active</c>, and is KYC verified.
    /// </remarks>
    [HttpPost("validate")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateAsync(
        [FromBody] ValidateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}

/// <summary>
/// Request payload used to update mutable customer profile fields.
/// </summary>
/// <param name="Name">Customer first name.</param>
/// <param name="Surname">Customer surname.</param>
/// <param name="TaxNumber">Corporate tax number when applicable.</param>
/// <param name="PhoneNumber">Customer phone number.</param>
/// <param name="DateOfBirth">Customer date of birth.</param>
/// <param name="Type">Customer type.</param>
public sealed record UpdateCustomerRequest(
    string Name,
    string Surname,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerService.Domain.Enums.CustomerType Type);

/// <summary>
/// Request payload used to change the operational status of a customer.
/// </summary>
/// <param name="Status">New status to apply to the customer.</param>
/// <param name="Reason">Optional reason recorded for the status change.</param>
public sealed record ChangeCustomerStatusRequest(
    CustomerService.Domain.Enums.CustomerStatus Status,
    string? Reason);
