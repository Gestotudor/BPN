using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Security;

public sealed class ApiKeyGenerator : IApiKeyGenerator
{
    public GeneratedApiKey Generate()
    {
        var rawValue = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var plainTextKey = $"mb_live_{rawValue}";
        var keyPrefix = plainTextKey[..Math.Min(plainTextKey.Length, 16)];
        var keyHash = ComputeHash(plainTextKey);

        return new GeneratedApiKey(plainTextKey, keyPrefix, keyHash);
    }

    public string ComputeHash(string plainTextApiKey)
    {
        var bytes = Encoding.UTF8.GetBytes(plainTextApiKey);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
