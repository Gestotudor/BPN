namespace AuthService.Application.Interfaces;

public interface IRateLimitService
{
    Task<bool> IsRequestAllowedAsync(
        string apiKeyIdentifier,
        CancellationToken cancellationToken = default);
}
