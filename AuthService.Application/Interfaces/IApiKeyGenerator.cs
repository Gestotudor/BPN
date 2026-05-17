namespace AuthService.Application.Interfaces;

public interface IApiKeyGenerator
{
    GeneratedApiKey Generate();

    string ComputeHash(string plainTextApiKey);
}

public sealed record GeneratedApiKey(string PlainTextKey, string KeyPrefix, string KeyHash);
