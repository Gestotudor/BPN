using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerResponse>>;
