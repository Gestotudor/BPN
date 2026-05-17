using FluentValidation;

namespace CustomerService.Application.Features.Customers.Commands.ChangeCustomerStatus;

public sealed class ChangeCustomerStatusCommandValidator : AbstractValidator<ChangeCustomerStatusCommand>
{
    public ChangeCustomerStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(250);
    }
}
