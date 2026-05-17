using TransferService.Application.Interfaces;

namespace TransferService.Infrastructure.Generators;

public sealed class TransactionCodeGenerator : ITransactionCodeGenerator
{
    private readonly ITransferRepository _transferRepository;

    public TransactionCodeGenerator(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var transactionCode = $"MB{Random.Shared.Next(10000000, 99999999)}";
            if (!await _transferRepository.ExistsByTransactionCodeAsync(transactionCode, cancellationToken))
            {
                return transactionCode;
            }
        }
    }
}
