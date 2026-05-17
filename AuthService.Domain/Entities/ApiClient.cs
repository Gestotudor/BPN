namespace AuthService.Domain.Entities;

public sealed class ApiClient
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
