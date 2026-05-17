using CustomerService.Application.Common.Mappings;
using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<DTOs.CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IKycVerificationLogRepository _kycVerificationLogRepository;
    private readonly IKycServiceClient _kycServiceClient;
    private readonly INationalIdValidator _nationalIdValidator;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IKycVerificationLogRepository kycVerificationLogRepository,
        IKycServiceClient kycServiceClient,
        INationalIdValidator nationalIdValidator,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _kycVerificationLogRepository = kycVerificationLogRepository;
        _kycServiceClient = kycServiceClient;
        _nationalIdValidator = nationalIdValidator;
        _logger = logger;
    }

    public async Task<Result<DTOs.CustomerResponse>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = Guid.NewGuid();

        if (CalculateAge(request.DateOfBirth) < 18)
        {
            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Validation,
                "Customers under 18 are not allowed.");
        }

        if (!_nationalIdValidator.IsValid(request.NationalIdNumber))
        {
            _logger.LogWarning("National ID validation failed for {NationalIdNumber}", request.NationalIdNumber);
            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Validation,
                "National ID number is invalid.");
        }

        if (await _customerRepository.ExistsByNationalIdNumberAsync(request.NationalIdNumber, cancellationToken))
        {
            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Conflict,
                "Customer already exists with the same national ID number.");
        }

        var kycResult = await _kycServiceClient.VerifyAsync(
            customerId,
            request.NationalIdNumber,
            request.Name,
            request.Surname,
            request.DateOfBirth,
            cancellationToken);

        if (kycResult.IsServiceUnavailable)
        {
            _logger.LogError("KYC service unavailable while creating customer {NationalIdNumber}", request.NationalIdNumber);
            await _kycVerificationLogRepository.AddAsync(
                new KycVerificationLog
                {
                    Id = Guid.NewGuid(),
                    NationalIdNumber = request.NationalIdNumber,
                    IsSuccess = false,
                    ErrorMessage = kycResult.ErrorMessage ?? "KYC service unavailable.",
                    ExternalReference = kycResult.ExternalReference,
                    RequestedAt = DateTime.UtcNow
                },
                cancellationToken);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.ServiceUnavailable,
                "KYC service is unavailable.");
        }

        if (!kycResult.IsSuccess)
        {
            _logger.LogWarning("KYC verification failed for {NationalIdNumber}", request.NationalIdNumber);
            await _kycVerificationLogRepository.AddAsync(
                new KycVerificationLog
                {
                    Id = Guid.NewGuid(),
                    NationalIdNumber = request.NationalIdNumber,
                    IsSuccess = false,
                    ErrorMessage = kycResult.ErrorMessage ?? "KYC verification failed.",
                    ExternalReference = kycResult.ExternalReference,
                    RequestedAt = DateTime.UtcNow
                },
                cancellationToken);
            await _customerRepository.SaveChangesAsync(cancellationToken);

            return Result<DTOs.CustomerResponse>.Failure(
                ResultErrorType.Validation,
                "Customer failed KYC verification.");
        }

        var customer = new Customer
        {
            Id = customerId,
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            NationalIdNumber = request.NationalIdNumber.Trim(),
            TaxNumber = string.IsNullOrWhiteSpace(request.TaxNumber) ? null : request.TaxNumber.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            Type = request.Type,
            Status = CustomerStatus.Active,
            IsKycVerified = true,
            KycVerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _kycVerificationLogRepository.AddAsync(
            new KycVerificationLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                NationalIdNumber = customer.NationalIdNumber,
                IsSuccess = true,
                ExternalReference = kycResult.ExternalReference,
                RequestedAt = DateTime.UtcNow
            },
            cancellationToken);

        await _customerRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created successfully for {NationalIdNumber}", customer.NationalIdNumber);
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
