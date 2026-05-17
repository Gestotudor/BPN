using CustomerService.Domain.Enums;

namespace CustomerService.Application.Features.Customers.DTOs;

/// <summary>
/// Detailed customer response returned by customer management endpoints.
/// </summary>
/// <param name="Id">Unique identifier of the customer.</param>
/// <param name="Name">Customer first name.</param>
/// <param name="Surname">Customer surname.</param>
/// <param name="FullName">Display name composed from the customer name fields.</param>
/// <param name="NationalIdNumber">National identity number recorded for the customer.</param>
/// <param name="TaxNumber">Tax number recorded for a corporate customer.</param>
/// <param name="PhoneNumber">Customer phone number.</param>
/// <param name="DateOfBirth">Customer date of birth.</param>
/// <param name="Type">Customer type.</param>
/// <param name="Status">Current lifecycle status of the customer.</param>
/// <param name="IsKycVerified">Indicates whether KYC verification succeeded.</param>
/// <param name="KycVerifiedAt">UTC timestamp when KYC verification succeeded.</param>
/// <param name="CreatedAt">UTC timestamp when the customer was created.</param>
/// <param name="UpdatedAt">UTC timestamp of the latest customer update.</param>
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
