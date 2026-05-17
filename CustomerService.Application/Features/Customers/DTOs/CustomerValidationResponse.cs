using CustomerService.Domain.Enums;

namespace CustomerService.Application.Features.Customers.DTOs;

public sealed record CustomerValidationResponse(
    bool IsValid,
    Guid? CustomerId,
    CustomerStatus? Status,
    bool IsKycVerified,
    string? FullName);
