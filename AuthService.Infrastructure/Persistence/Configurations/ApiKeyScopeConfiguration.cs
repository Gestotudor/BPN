using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public sealed class ApiKeyScopeConfiguration : IEntityTypeConfiguration<ApiKeyScope>
{
    public void Configure(EntityTypeBuilder<ApiKeyScope> builder)
    {
        builder.ToTable("api_key_scopes");

        builder.HasKey(x => new { x.ApiKeyId, x.ScopeId });

        builder.HasOne(x => x.ApiKey)
            .WithMany(x => x.ApiKeyScopes)
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Scope)
            .WithMany(x => x.ApiKeyScopes)
            .HasForeignKey(x => x.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
