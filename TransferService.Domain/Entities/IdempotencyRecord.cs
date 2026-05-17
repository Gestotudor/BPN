using BuildingBlocks.Entities;

namespace TransferService.Domain.Entities;

public class IdempotencyRecord : BaseEntity
{
    public string IdempotencyKey { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public string ResponseJson { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }
}
