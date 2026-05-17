using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Enums;
using MediatR;

namespace CustomerService.Application.Features.Customers.Queries.ValidateCustomer;

public sealed class ValidateCustomerCommandHandler
    : IRequestHandler<ValidateCustomerCommand, Result<DTOs.CustomerValidationResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public ValidateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<DTOs.CustomerValidationResponse>> Handle(
        ValidateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result<DTOs.CustomerValidationResponse>.Success(
                new DTOs.CustomerValidationResponse(false, null, null, false, null));
        }

        var isValid = customer.Status == CustomerStatus.Active && customer.IsKycVerified;

        return Result<DTOs.CustomerValidationResponse>.Success(
            new DTOs.CustomerValidationResponse(
                isValid,
                customer.Id,
                customer.Status,
                customer.IsKycVerified,
                $"{customer.Name} {customer.Surname}"));
    }
}
