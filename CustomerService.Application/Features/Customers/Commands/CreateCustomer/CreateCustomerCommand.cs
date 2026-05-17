using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Domain.Enums;
using MediatR;

namespace CustomerService.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Request payload used to register a new customer after KYC verification.
/// </summary>
/// <param name="Name">Customer first name or company contact first name.</param>
/// <param name="Surname">Customer surname or company contact surname.</param>
/// <param name="NationalIdNumber">National identity number for an individual customer.</param>
/// <param name="TaxNumber">Tax number for a corporate customer.</param>
/// <param name="PhoneNumber">Customer phone number used for operational contact.</param>
/// <param name="DateOfBirth">Customer date of birth.</param>
/// <param name="Type">Customer type that determines required identifiers and validation rules.</param>
public sealed record CreateCustomerCommand(
    string Name,
    string Surname,
    string NationalIdNumber,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerType Type) : IRequest<Result<CustomerResponse>>;
