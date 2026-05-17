using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public sealed class ApiScopeConfiguration : IEntityTypeConfiguration<ApiScope>
{
    public void Configure(EntityTypeBuilder<ApiScope> builder)
    {
        builder.ToTable("api_scopes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.HasData(
            new ApiScope { Id = Guid.Parse("d11dc76f-c426-4ef8-bf0d-4d9fb4e14501"), Name = "customer.read", Description = "Read customer data" },
            new ApiScope { Id = Guid.Parse("da1842c9-13a4-414d-ae47-882ee3964e18"), Name = "customer.write", Description = "Write customer data" },
            new ApiScope { Id = Guid.Parse("a4df7a08-1f9b-4470-a76c-f90ccfdb2f1b"), Name = "transfer.read", Description = "Read transfer data" },
            new ApiScope { Id = Guid.Parse("ca12d43d-c1b2-4dda-a4f9-f3349a95b12d"), Name = "transfer.write", Description = "Write transfer data" });
    }
}
