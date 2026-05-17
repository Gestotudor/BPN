using CustomerService.Domain.Enums;

namespace CustomerService.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string NationalIdNumber { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public CustomerType Type { get; set; }

    public CustomerStatus Status { get; set; }

    public bool IsKycVerified { get; set; }

    public DateTime? KycVerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<CustomerStatusHistory> StatusHistory { get; set; } = new List<CustomerStatusHistory>();
}
