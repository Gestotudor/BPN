using BuildingBlocks.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Domain.Entities;

public class TransferStatusHistory : BaseEntity
{
    public Guid TransferId { get; set; }

    public Transfer Transfer { get; set; } = null!;

    public TransferStatus? OldStatus { get; set; }

    public TransferStatus NewStatus { get; set; }

    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }
}
