using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public sealed class ApiKeyRepository : IApiKeyRepository
{
    private readonly AuthDbContext _dbContext;

    public ApiKeyRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
    }

    public Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ApiKeys
            .Include(x => x.Client)
            .Include(x => x.ApiKeyScopes)
            .ThenInclude(x => x.Scope)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.ApiKeys
            .Include(x => x.Client)
            .Include(x => x.ApiKeyScopes)
            .ThenInclude(x => x.Scope)
            .FirstOrDefaultAsync(x => x.KeyHash == keyHash, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ApiKey>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApiKeys
            .Include(x => x.Client)
            .Include(x => x.ApiKeyScopes)
            .ThenInclude(x => x.Scope)
            .Where(x => x.IsActive && x.RevokedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
