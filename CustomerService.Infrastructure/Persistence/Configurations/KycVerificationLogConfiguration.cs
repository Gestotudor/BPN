using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class KycVerificationLogConfiguration : IEntityTypeConfiguration<KycVerificationLog>
{
    public void Configure(EntityTypeBuilder<KycVerificationLog> builder)
    {
        builder.ToTable("kyc_verification_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NationalIdNumber).IsRequired().HasMaxLength(11);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Property(x => x.RequestedAt).IsRequired();
    }
}
