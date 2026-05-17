using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public sealed class ApiClientRepository : IApiClientRepository
{
    private readonly AuthDbContext _dbContext;

    public ApiClientRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApiClient?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.ApiClients
            .Include(x => x.ApiKeys)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task AddAsync(ApiClient client, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApiClients.AddAsync(client, cancellationToken);
    }
}
