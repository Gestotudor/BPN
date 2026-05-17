using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Domain.Enums;
using MediatR;

namespace CustomerService.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Surname,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerType Type) : IRequest<Result<CustomerResponse>>;
