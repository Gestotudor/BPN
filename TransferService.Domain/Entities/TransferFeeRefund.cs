using BuildingBlocks.Entities;

namespace TransferService.Domain.Entities;

public class TransferFeeRefund : BaseEntity
{
    public Guid TransferId { get; set; }

    public Transfer Transfer { get; set; } = null!;

    public decimal RefundedFeeAmount { get; set; }

    public DateTime RefundedAt { get; set; }

    public string? Reason { get; set; }
}
