using TransferService.Domain.Entities;

namespace TransferService.Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
}
