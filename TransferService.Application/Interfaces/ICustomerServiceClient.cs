using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Interfaces;

public interface ICustomerServiceClient
{
    Task<CustomerResolutionResult> ValidateAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerResolutionResult> EnsureValidatedAsync(
        TransferCustomerRequest customer,
        CancellationToken cancellationToken = default);
}

public sealed record CustomerResolutionResult(
    bool IsSuccess,
    Guid? CustomerId,
    bool IsValid,
    int? Status,
    bool IsKycVerified,
    string? FullName,
    ResultErrorType? ErrorType,
    string? ErrorMessage,
    bool IsUnauthorized);
