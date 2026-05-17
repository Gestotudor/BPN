using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Persistence;

public sealed class AuthDbContextSeeder
{
    private const string AdminApiKeyPath = "Seed:AdminApiKey";
    private const string AdminClientNamePath = "Seed:AdminClientName";

    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IApiKeyGenerator _apiKeyGenerator;

    public AuthDbContextSeeder(
        AuthDbContext dbContext,
        IConfiguration configuration,
        IApiKeyGenerator apiKeyGenerator)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _apiKeyGenerator = apiKeyGenerator;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        var adminApiKey = _configuration[AdminApiKeyPath];
        var adminClientName = _configuration[AdminClientNamePath] ?? "MoneyBee Admin";

        if (string.IsNullOrWhiteSpace(adminApiKey))
        {
            return;
        }

        var keyHash = _apiKeyGenerator.ComputeHash(adminApiKey);
        var existingKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(x => x.KeyHash == keyHash, cancellationToken);

        if (existingKey is not null)
        {
            return;
        }

        var client = await _dbContext.ApiClients.FirstOrDefaultAsync(x => x.Name == adminClientName, cancellationToken);

        if (client is null)
        {
            client = new ApiClient
            {
                Id = Guid.NewGuid(),
                Name = adminClientName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.ApiClients.AddAsync(client, cancellationToken);
        }

        var scopes = await _dbContext.ApiScopes.ToListAsync(cancellationToken);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            KeyPrefix = adminApiKey[..Math.Min(adminApiKey.Length, 16)],
            KeyHash = keyHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var scope in scopes)
        {
            apiKey.ApiKeyScopes.Add(new ApiKeyScope
            {
                ApiKeyId = apiKey.Id,
                ApiKey = apiKey,
                ScopeId = scope.Id,
                Scope = scope
            });
        }

        await _dbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
