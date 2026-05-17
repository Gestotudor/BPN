using BuildingBlocks.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Domain.Entities;

public class Transfer : BaseEntity
{
    public Guid SenderCustomerId { get; set; }

    public Guid ReceiverCustomerId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "TRY";

    public decimal TryAmount { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal Fee { get; set; }

    public string TransactionCode { get; set; } = null!;

    public TransferStatus Status { get; set; }

    public FraudRiskLevel FraudRiskLevel { get; set; } = FraudRiskLevel.Low;

    public DateTime? ApprovalAvailableAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public ICollection<TransferStatusHistory> StatusHistory { get; set; } = new List<TransferStatusHistory>();

    public ICollection<TransferFeeRefund> FeeRefunds { get; set; } = new List<TransferFeeRefund>();
}
