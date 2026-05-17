using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Surname).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NationalIdNumber).IsRequired().HasMaxLength(11);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.TaxNumber).HasMaxLength(20);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.NationalIdNumber).IsUnique();
        builder.HasIndex(x => x.PhoneNumber);
        builder.HasIndex(x => x.Status);
    }
}
