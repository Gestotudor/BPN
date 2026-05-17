using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.KeyPrefix)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.KeyHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.KeyHash)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.RevokedReason)
            .HasMaxLength(250);

        builder.HasOne(x => x.Client)
            .WithMany(x => x.ApiKeys)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
