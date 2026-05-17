using FluentValidation;

namespace AuthService.Application.Features.ApiKeys.Commands.RevokeApiKey;

public sealed class RevokeApiKeyCommandValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(250);
    }
}
