namespace TransferService.Application.Interfaces;

public interface ITransactionCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
