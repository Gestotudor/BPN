namespace AuthService.Domain.Entities;

public sealed class ApiKeyScope
{
    public Guid ApiKeyId { get; set; }

    public ApiKey ApiKey { get; set; } = null!;

    public Guid ScopeId { get; set; }

    public ApiScope Scope { get; set; } = null!;
}
