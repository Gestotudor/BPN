using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IApiClientRepository
{
    Task<ApiClient?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(ApiClient client, CancellationToken cancellationToken = default);
}
