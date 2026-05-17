using TransferService.Domain.Entities;

namespace TransferService.Application.Interfaces;

public interface ITransferRepository
{
    Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);

    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transfer?> GetByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default);

    Task<decimal> GetDailyTotalAmountAsync(
        Guid senderCustomerId,
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default);

    Task<List<Transfer>> GetPendingTransfersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
