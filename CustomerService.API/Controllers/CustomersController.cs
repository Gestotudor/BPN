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

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("by-national-id/{nationalIdNumber}")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNationalIdAsync(string nationalIdNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByNationalIdQuery(nationalIdNumber), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
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

    [HttpPatch("{id:guid}/status")]
    [RequiredScopes("customer.write")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
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

    [HttpPost("validate")]
    [RequiredScopes("customer.read")]
    [ProducesResponseType(typeof(ApiResponse<CustomerValidationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateAsync(
        [FromBody] ValidateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}

public sealed record UpdateCustomerRequest(
    string Name,
    string Surname,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerService.Domain.Enums.CustomerType Type);

public sealed record ChangeCustomerStatusRequest(
    CustomerService.Domain.Enums.CustomerStatus Status,
    string? Reason);
