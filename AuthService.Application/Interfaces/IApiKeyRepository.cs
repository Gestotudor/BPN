using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IApiKeyRepository
{
    Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default);

    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ApiKey>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
