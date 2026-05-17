using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TransferService.Application.Interfaces;

namespace TransferService.Infrastructure.ExternalServices.Exchange;

public sealed class ExchangeRateClient : IExchangeRateClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateClient> _logger;

    public ExchangeRateClient(HttpClient httpClient, ILogger<ExchangeRateClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExchangeRateResult> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/exchange/rate/{fromCurrency}/{toCurrency}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Exchange rate could not be retrieved.");
            }

            var result = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>(cancellationToken: cancellationToken);
            if (result is null)
            {
                throw new InvalidOperationException("Exchange rate response was empty.");
            }

            return new ExchangeRateResult(result.Rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exchange rate service failed for {FromCurrency}/{ToCurrency}", fromCurrency, toCurrency);
            throw;
        }
    }
}
