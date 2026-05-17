using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.ValidateCustomer;

public sealed record ValidateCustomerCommand(Guid CustomerId) : IRequest<Result<CustomerValidationResponse>>;
