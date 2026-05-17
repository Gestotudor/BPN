using FluentValidation;

namespace CustomerService.Application.Features.Customers.Queries.ValidateCustomer;

public sealed class ValidateCustomerCommandValidator : AbstractValidator<ValidateCustomerCommand>
{
    public ValidateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
