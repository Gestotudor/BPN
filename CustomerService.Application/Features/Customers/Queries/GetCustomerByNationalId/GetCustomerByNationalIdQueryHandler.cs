using CustomerService.Application.Common.Mappings;
using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.GetCustomerByNationalId;

public sealed class GetCustomerByNationalIdQueryHandler
    : IRequestHandler<GetCustomerByNationalIdQuery, Result<DTOs.CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByNationalIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<DTOs.CustomerResponse>> Handle(
        GetCustomerByNationalIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByNationalIdNumberAsync(
            request.NationalIdNumber,
            cancellationToken);

        return customer is null
            ? Result<DTOs.CustomerResponse>.Failure(ResultErrorType.NotFound, "Customer not found.")
            : Result<DTOs.CustomerResponse>.Success(customer.ToResponse());
    }
}
