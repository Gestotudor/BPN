namespace AuthService.Domain.Entities;

public sealed class ApiScope
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<ApiKeyScope> ApiKeyScopes { get; set; } = new List<ApiKeyScope>();
}
