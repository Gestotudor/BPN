using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Domain.Enums;
using MediatR;

namespace CustomerService.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string Surname,
    string NationalIdNumber,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerType Type) : IRequest<Result<CustomerResponse>>;
