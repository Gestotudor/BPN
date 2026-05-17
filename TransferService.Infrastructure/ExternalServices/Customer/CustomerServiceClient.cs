using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;

namespace TransferService.Infrastructure.ExternalServices.Customer;

public sealed class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomerServiceClient> _logger;

    public CustomerServiceClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomerServiceClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<CustomerResolutionResult> EnsureValidatedAsync(
        TransferCustomerRequest customer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedCustomer = Normalize(customer);
            var customerResponse = await GetByNationalIdAsync(normalizedCustomer.NationalIdNumber, cancellationToken);
            if (customerResponse.IsUnauthorized)
            {
                return Forbidden("Transfer API key is not authorized to access customer service.");
            }

            if (!customerResponse.IsSuccess && customerResponse.ErrorType != ResultErrorType.NotFound)
            {
                return customerResponse;
            }

            CustomerResponse? ensuredCustomer;
            if (customerResponse.IsSuccess && customerResponse.CustomerId.HasValue)
            {
                var updateResult = await UpdateAsync(customerResponse.CustomerId.Value, normalizedCustomer, cancellationToken);
                if (!updateResult.IsSuccess)
                {
                    return updateResult;
                }

                ensuredCustomer = new CustomerResponse(
                    customerResponse.CustomerId.Value,
                    normalizedCustomer.Name,
                    normalizedCustomer.Surname,
                    $"{normalizedCustomer.Name} {normalizedCustomer.Surname}",
                    normalizedCustomer.NationalIdNumber,
                    normalizedCustomer.TaxNumber,
                    normalizedCustomer.PhoneNumber,
                    normalizedCustomer.DateOfBirth,
                    (int)normalizedCustomer.Type);
            }
            else
            {
                var createResult = await CreateAsync(normalizedCustomer, cancellationToken);
                if (!createResult.IsSuccess)
                {
                    return createResult.Error!;
                }

                ensuredCustomer = createResult.Customer;
            }

            return await ValidateAsync(ensuredCustomer!.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer resolution failed for {NationalIdNumber}", customer.NationalIdNumber);
            return Failure(ResultErrorType.ServiceUnavailable, "Customer service request failed.");
        }
    }

    private async Task<CustomerResolutionResult> GetByNationalIdAsync(string nationalIdNumber, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/customers/by-national-id/{Uri.EscapeDataString(nationalIdNumber)}");
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Customer lookup unauthorized for {NationalIdNumber}", nationalIdNumber);
            return Forbidden("Customer lookup is not authorized.");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Failure(ResultErrorType.NotFound, "Customer not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Customer lookup failed for {NationalIdNumber}. StatusCode: {StatusCode}", nationalIdNumber, (int)response.StatusCode);
            return await FailureFromResponseAsync(response, "Customer lookup failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiEnvelope<CustomerResponse>>(cancellationToken: cancellationToken);
        var data = result?.Data;
        if (data is null)
        {
            return Failure(ResultErrorType.ServiceUnavailable, "Customer lookup response could not be read.");
        }

        return new CustomerResolutionResult(true, data.Id, false, null, false, data.FullName, null, null, false);
    }

    private async Task<CustomerMutationResult> CreateAsync(TransferCustomerRequest customer, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/customers");
        request.Content = JsonContent.Create(new CustomerMutationRequest(
            customer.Name,
            customer.Surname,
            customer.NationalIdNumber,
            customer.TaxNumber,
            customer.PhoneNumber,
            customer.DateOfBirth,
            (int)customer.Type));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Customer create unauthorized for {NationalIdNumber}", customer.NationalIdNumber);
            return CustomerMutationResult.FromResolution(Forbidden("Customer create is not authorized."));
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Customer create failed for {NationalIdNumber}. StatusCode: {StatusCode}", customer.NationalIdNumber, (int)response.StatusCode);
            return CustomerMutationResult.FromResolution(await FailureFromResponseAsync(response, "Customer create failed."));
        }

        var result = await response.Content.ReadFromJsonAsync<ApiEnvelope<CustomerResponse>>(cancellationToken: cancellationToken);
        return result?.Data is null
            ? CustomerMutationResult.FromResolution(Failure(ResultErrorType.ServiceUnavailable, "Customer create response could not be read."))
            : new CustomerMutationResult(true, result.Data, null);
    }

    private async Task<CustomerResolutionResult> UpdateAsync(Guid customerId, TransferCustomerRequest customer, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Put, $"/api/customers/{customerId}");
        request.Content = JsonContent.Create(new CustomerUpdateRequest(
            customer.Name,
            customer.Surname,
            customer.TaxNumber,
            customer.PhoneNumber,
            customer.DateOfBirth,
            (int)customer.Type));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Customer update unauthorized for {CustomerId}", customerId);
            return Forbidden("Customer update is not authorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Customer update failed for {CustomerId}. StatusCode: {StatusCode}", customerId, (int)response.StatusCode);
            return await FailureFromResponseAsync(response, "Customer update failed.");
        }

        return new CustomerResolutionResult(true, customerId, false, null, false, null, null, null, false);
    }

    public async Task<CustomerResolutionResult> ValidateAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/customers/validate");
        request.Content = JsonContent.Create(new { CustomerId = customerId });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Customer validation unauthorized for {CustomerId}", customerId);
            return Forbidden("Customer validation is not authorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Customer validation failed for {CustomerId}. StatusCode: {StatusCode}", customerId, (int)response.StatusCode);
            return await FailureFromResponseAsync(response, "Customer validation failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiEnvelope<CustomerValidateResponse>>(cancellationToken: cancellationToken);
        var data = result?.Data;
        if (data is null)
        {
            return Failure(ResultErrorType.ServiceUnavailable, "Customer validation response could not be read.");
        }

        return new CustomerResolutionResult(
            true,
            data.CustomerId,
            data.IsValid,
            data.Status,
            data.IsKycVerified,
            data.FullName,
            null,
            null,
            false);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri);
        var apiKey = _httpContextAccessor.HttpContext?.Request.Headers["X-API-Key"].FirstOrDefault()
            ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Api-Key"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("X-API-Key", apiKey);
        }

        return request;
    }

    private static TransferCustomerRequest Normalize(TransferCustomerRequest customer) => customer with
    {
        Name = customer.Name.Trim(),
        Surname = customer.Surname.Trim(),
        NationalIdNumber = customer.NationalIdNumber.Trim(),
        TaxNumber = string.IsNullOrWhiteSpace(customer.TaxNumber) ? null : customer.TaxNumber.Trim(),
        PhoneNumber = customer.PhoneNumber.Trim()
    };

    private static CustomerResolutionResult Forbidden(string message) =>
        new(false, null, false, null, false, null, ResultErrorType.Forbidden, message, true);

    private static CustomerResolutionResult Failure(ResultErrorType errorType, string message) =>
        new(false, null, false, null, false, null, errorType, message, false);

    private static async Task<CustomerResolutionResult> FailureFromResponseAsync(HttpResponseMessage response, string fallbackMessage)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();
        var message = string.IsNullOrWhiteSpace(error?.Message) ? fallbackMessage : error.Message;

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => Failure(ResultErrorType.Validation, message!),
            System.Net.HttpStatusCode.Conflict => Failure(ResultErrorType.Conflict, message!),
            System.Net.HttpStatusCode.NotFound => Failure(ResultErrorType.NotFound, message!),
            _ => Failure(ResultErrorType.ServiceUnavailable, message!)
        };
    }

    private sealed record ApiEnvelope<T>(bool Success, string? Message, T? Data);

    private sealed record CustomerResponse(
        Guid Id,
        string Name,
        string Surname,
        string FullName,
        string NationalIdNumber,
        string? TaxNumber,
        string PhoneNumber,
        DateTime DateOfBirth,
        int Type);

    private sealed record CustomerMutationRequest(
        string Name,
        string Surname,
        string NationalIdNumber,
        string? TaxNumber,
        string PhoneNumber,
        DateTime DateOfBirth,
        int Type);

    private sealed record CustomerUpdateRequest(
        string Name,
        string Surname,
        string? TaxNumber,
        string PhoneNumber,
        DateTime DateOfBirth,
        int Type);

    private sealed record CustomerValidateResponse(
        bool IsValid,
        Guid? CustomerId,
        int? Status,
        bool IsKycVerified,
        string? FullName);

    private sealed record CustomerMutationResult(bool IsSuccess, CustomerResponse? Customer, CustomerResolutionResult? Error)
    {
        public static CustomerMutationResult FromResolution(CustomerResolutionResult error) => new(false, null, error);
    }
}
