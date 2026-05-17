using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class CustomerStatusHistoryConfiguration : IEntityTypeConfiguration<CustomerStatusHistory>
{
    public void Configure(EntityTypeBuilder<CustomerStatusHistory> builder)
    {
        builder.ToTable("customer_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(250);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
