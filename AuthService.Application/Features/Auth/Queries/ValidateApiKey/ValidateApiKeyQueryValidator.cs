using FluentValidation;

namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

public sealed class ValidateApiKeyQueryValidator : AbstractValidator<ValidateApiKeyQuery>
{
    public ValidateApiKeyQueryValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .Matches("^mb_live_[A-Za-z0-9]+$")
            .WithMessage("Api key format is invalid.");

        RuleForEach(x => x.RequiredScopes)
            .Matches("^[a-z]+\\.[a-z]+$")
            .When(x => x.RequiredScopes is { Count: > 0 })
            .WithMessage("Scope format is invalid.");
    }
}
