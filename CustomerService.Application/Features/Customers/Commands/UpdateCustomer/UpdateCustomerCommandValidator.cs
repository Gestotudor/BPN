using CustomerService.Domain.Enums;
using FluentValidation;

namespace CustomerService.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.TaxNumber)
            .NotEmpty()
            .When(x => x.Type == CustomerType.Corporate)
            .WithMessage("Tax number is required for corporate customers.");
    }
}
