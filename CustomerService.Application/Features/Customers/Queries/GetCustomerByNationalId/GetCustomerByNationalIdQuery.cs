using CustomerService.Application.Common.Results;
using CustomerService.Application.Features.Customers.DTOs;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.GetCustomerByNationalId;

public sealed record GetCustomerByNationalIdQuery(string NationalIdNumber) : IRequest<Result<CustomerResponse>>;
