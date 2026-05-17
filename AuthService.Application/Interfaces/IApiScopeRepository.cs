using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IApiScopeRepository
{
    Task<IReadOnlyCollection<ApiScope>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);
}
