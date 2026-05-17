using CustomerService.Application.Common.Mappings;
using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<DTOs.CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<DTOs.CustomerResponse>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        return customer is null
            ? Result<DTOs.CustomerResponse>.Failure(ResultErrorType.NotFound, "Customer not found.")
            : Result<DTOs.CustomerResponse>.Success(customer.ToResponse());
    }
}
