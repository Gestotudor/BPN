using FluentValidation;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Commands.CreateTransfer;

public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.SenderCustomer)
            .NotNull()
            .SetValidator(new TransferCustomerRequestValidator());

        RuleFor(x => x.ReceiverCustomer)
            .NotNull()
            .SetValidator(new TransferCustomerRequestValidator());

        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x)
            .Must(x =>
                x.SenderCustomer is not null &&
                x.ReceiverCustomer is not null &&
                !string.Equals(
                    NormalizeNationalId(x.SenderCustomer.NationalIdNumber),
                    NormalizeNationalId(x.ReceiverCustomer.NationalIdNumber),
                    StringComparison.Ordinal))
            .WithMessage("Sender and receiver cannot be same.");
    }

    private static string NormalizeNationalId(string nationalIdNumber) => nationalIdNumber.Trim();
}

public sealed class TransferCustomerRequestValidator : AbstractValidator<TransferCustomerRequest>
{
    public TransferCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NationalIdNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.TaxNumber)
            .NotEmpty()
            .When(x => x.Type == TransferCustomerType.Corporate)
            .WithMessage("Tax number is required for corporate customers.");
    }
}
