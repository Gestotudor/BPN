using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TransferService.Application.Interfaces;
using TransferService.Domain.Enums;

namespace TransferService.Infrastructure.ExternalServices.Fraud;

public sealed class FraudDetectionClient : IFraudDetectionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FraudDetectionClient> _logger;

    public FraudDetectionClient(HttpClient httpClient, ILogger<FraudDetectionClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FraudCheckResult> CheckAsync(
        string transactionCode,
        Guid senderCustomerId,
        Guid receiverCustomerId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/fraud/check",
                new FraudCheckRequest
                {
                    TransactionId = transactionCode,
                    UserId = senderCustomerId.ToString(),
                    ToUserId = receiverCustomerId.ToString(),
                    Amount = amount,
                    Currency = currency
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Fraud check failed.");
            }

            var result = await response.Content.ReadFromJsonAsync<FraudCheckResponse>(cancellationToken: cancellationToken);
            if (result?.Success != true || result.Data is null)
            {
                throw new InvalidOperationException("Fraud check payload was invalid.");
            }

            return new FraudCheckResult(
                string.Equals(result.Data.RiskLevel, "HIGH", StringComparison.OrdinalIgnoreCase)
                    ? FraudRiskLevel.High
                    : FraudRiskLevel.Low,
                result.Data.RiskScore,
                result.Data.ShouldBlock,
                result.Data.RiskFactors,
                result.Data.Recommendations,
                result.Data.RequiredActions,
                result.Data.ProcessingTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fraud service failed for transaction {TransactionCode}", transactionCode);
            throw;
        }
    }
}
