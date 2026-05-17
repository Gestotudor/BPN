using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApiClient> ApiClients => Set<ApiClient>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<ApiScope> ApiScopes => Set<ApiScope>();

    public DbSet<ApiKeyScope> ApiKeyScopes => Set<ApiKeyScope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
