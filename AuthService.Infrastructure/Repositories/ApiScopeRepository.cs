using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public sealed class ApiScopeRepository : IApiScopeRepository
{
    private readonly AuthDbContext _dbContext;

    public ApiScopeRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ApiScope>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        var normalizedNames = names
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return await _dbContext.ApiScopes
            .Where(x => normalizedNames.Contains(x.Name))
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }
}
