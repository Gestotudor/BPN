namespace TransferService.Application.Interfaces;

public interface IExchangeRateClient
{
    Task<ExchangeRateResult> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default);
}

public sealed record ExchangeRateResult(decimal Rate);
