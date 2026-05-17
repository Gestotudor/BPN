using CustomerService.Application.Common.Mappings;
using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<DTOs.CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<UpdateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Result<DTOs.CustomerResponse>> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<DTOs.CustomerResponse>.Failure(ResultErrorType.NotFound, "Customer not found.");
        }

        if (CalculateAge(request.DateOfBirth) < 18)
        {
            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Validation,
                "Customers under 18 are not allowed.");
        }

        if (request.Type == CustomerType.Corporate && string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Validation,
                "Tax number is required for corporate customers.");
        }

        customer.Name = request.Name.Trim();
        customer.Surname = request.Surname.Trim();
        customer.PhoneNumber = request.PhoneNumber.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.Type = request.Type;
        customer.TaxNumber = string.IsNullOrWhiteSpace(request.TaxNumber) ? null : request.TaxNumber.Trim();
        customer.UpdatedAt = DateTime.UtcNow;

        await _customerRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} updated.", customer.Id);
        return Result<DTOs.CustomerResponse>.Success(customer.ToResponse());
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Date.Year;

        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
