using CustomerService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CustomerService.Infrastructure.ExternalServices.Kyc;

public sealed class KycServiceClient : IKycServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KycServiceClient> _logger;

    public KycServiceClient(HttpClient httpClient, ILogger<KycServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<KycVerificationResult> VerifyAsync(
        Guid userId,
        string nationalIdNumber,
        string name,
        string surname,
        DateTime dateOfBirth,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new KycVerifyRequest(
                userId.ToString(),
                nationalIdNumber,
                dateOfBirth.Year,
                name,
                surname);

            HttpResponseMessage? response = null;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                try
                {
                    response = await _httpClient.PostAsJsonAsync("/api/kyc/verify", payload, timeoutCts.Token);
                    break;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    _logger.LogWarning(ex, "KYC call attempt {Attempt} failed.", attempt + 1);

                    if (attempt == 2)
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(250 * (attempt + 1)), cancellationToken);
                }
            }

            if (response is null)
            {
                return new KycVerificationResult(false, "KYC service unavailable.", null, true);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("KYC service returned non-success status code {StatusCode}", response.StatusCode);
                return new KycVerificationResult(false, "KYC verification failed.", null, false);
            }

            var result = await response.Content.ReadFromJsonAsync<KycVerifyResponse>(cancellationToken: cancellationToken);

            return new KycVerificationResult(
                result?.Success == true,
                result?.Success == true ? null : result?.Message ?? "KYC verification failed.",
                result?.ReferenceId,
                false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "KYC service unavailable.");
            return new KycVerificationResult(false, "KYC service unavailable.", null, true);
        }
    }
}
