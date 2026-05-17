namespace AuthService.Domain.Entities;

public sealed class ApiKey
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public ApiClient Client { get; set; } = null!;

    public string KeyPrefix { get; set; } = string.Empty;

    public string KeyHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public ICollection<ApiKeyScope> ApiKeyScopes { get; set; } = new List<ApiKeyScope>();
}
