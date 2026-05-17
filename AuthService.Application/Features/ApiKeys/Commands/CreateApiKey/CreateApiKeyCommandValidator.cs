using FluentValidation;

namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Scopes)
            .NotEmpty();

        RuleForEach(x => x.Scopes)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ExpiresAt)
            .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
            .WithMessage("ExpiresAt must be a future UTC date.");
    }
}
