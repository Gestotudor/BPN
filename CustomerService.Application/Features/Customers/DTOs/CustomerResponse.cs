using CustomerService.Domain.Enums;

namespace CustomerService.Application.Features.Customers.DTOs;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string Surname,
    string FullName,
    string NationalIdNumber,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    CustomerType Type,
    CustomerStatus Status,
    bool IsKycVerified,
    DateTime? KycVerifiedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
