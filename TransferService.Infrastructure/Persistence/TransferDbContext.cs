using Microsoft.EntityFrameworkCore;
using TransferService.Domain.Entities;

namespace TransferService.Infrastructure.Persistence;

public class TransferDbContext : DbContext
{
    public TransferDbContext(DbContextOptions<TransferDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<TransferStatusHistory> TransferStatusHistories => Set<TransferStatusHistory>();

    public DbSet<TransferFeeRefund> TransferFeeRefunds => Set<TransferFeeRefund>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("transfers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.TryAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.Fee).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.TransactionCode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TransactionCode).IsUnique();
            entity.HasIndex(x => x.SenderCustomerId);
            entity.HasIndex(x => x.ReceiverCustomerId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<TransferStatusHistory>(entity =>
        {
            entity.ToTable("transfer_status_histories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.HasOne(x => x.Transfer)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.TransferId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransferFeeRefund>(entity =>
        {
            entity.ToTable("transfer_fee_refunds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefundedFeeAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.HasOne(x => x.Transfer)
                .WithMany(x => x.FeeRefunds)
                .HasForeignKey(x => x.TransferId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable("idempotency_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RequestHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ResponseJson).HasColumnType("text").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        });
    }
}
