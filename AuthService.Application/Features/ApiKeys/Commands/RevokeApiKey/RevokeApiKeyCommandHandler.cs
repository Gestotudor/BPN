using AuthService.Application.Common.Results;
using AuthService.Application.Interfaces;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Commands.RevokeApiKey;

public sealed class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, Result>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public RevokeApiKeyCommandHandler(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (apiKey is null)
        {
            return Result.Failure(ResultErrorType.NotFound, "API key not found.");
        }

        apiKey.IsActive = false;
        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.RevokedReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Revoked by request."
            : request.Reason.Trim();

        await _apiKeyRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
