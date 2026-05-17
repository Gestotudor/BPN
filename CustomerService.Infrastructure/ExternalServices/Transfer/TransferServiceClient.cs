using CustomerService.Application.Interfaces;
using CustomerService.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CustomerService.Infrastructure.ExternalServices.Transfer;

public sealed class TransferServiceClient : ITransferServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TransferServiceClient> _logger;

    public TransferServiceClient(HttpClient httpClient, ILogger<TransferServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyCustomerStatusChangedAsync(
        Guid customerId,
        CustomerStatus oldStatus,
        CustomerStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/internal/customer-status-changed",
            new CustomerStatusChangedRequest(customerId, oldStatus.ToString(), newStatus.ToString()),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Transfer service notification failed for customer {CustomerId} with status code {StatusCode}",
                customerId,
                response.StatusCode);

            throw new HttpRequestException("Transfer service notification failed.");
        }
    }
}
