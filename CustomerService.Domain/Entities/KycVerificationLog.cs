namespace CustomerService.Domain.Entities;

public sealed class KycVerificationLog
{
    public Guid Id { get; set; }

    public Guid? CustomerId { get; set; }

    public string NationalIdNumber { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ExternalReference { get; set; }

    public DateTime RequestedAt { get; set; }
}
