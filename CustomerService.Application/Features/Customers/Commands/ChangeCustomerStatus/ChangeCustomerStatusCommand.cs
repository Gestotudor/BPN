using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Domain.Enums;
using MediatR;

namespace CustomerService.Application.Features.Customers.Commands.ChangeCustomerStatus;

public sealed record ChangeCustomerStatusCommand(
    Guid Id,
    CustomerStatus Status,
    string? Reason) : IRequest<Result<CustomerResponse>>;
