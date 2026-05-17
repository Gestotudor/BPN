using CustomerService.Domain.Enums;
using FluentValidation;

namespace CustomerService.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.NationalIdNumber).NotEmpty().Length(11).Matches("^\\d{11}$");
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.TaxNumber)
            .NotEmpty()
            .When(x => x.Type == CustomerType.Corporate)
            .WithMessage("Tax number is required for corporate customers.");
    }
}
