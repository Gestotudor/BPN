using CustomerService.Domain.Enums;

namespace CustomerService.Application.Features.Customers.DTOs;

/// <summary>
/// Validation result returned to the Transfer service before a transfer is processed.
/// </summary>
/// <param name="IsValid">Indicates whether the customer can participate in a transfer.</param>
/// <param name="CustomerId">Identifier of the customer when validation succeeds.</param>
/// <param name="Status">Current status of the customer.</param>
/// <param name="IsKycVerified">Indicates whether KYC verification has been completed successfully.</param>
/// <param name="FullName">Display name of the customer.</param>
public sealed record CustomerValidationResponse(
    bool IsValid,
    Guid? CustomerId,
    CustomerStatus? Status,
    bool IsKycVerified,
    string? FullName);
