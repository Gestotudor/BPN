using FluentValidation;

namespace TransferService.Application.Features.Transfers.Commands.ReceiveTransfer;

public sealed class ReceiveTransferCommandValidator : AbstractValidator<ReceiveTransferCommand>
{
    public ReceiveTransferCommandValidator()
    {
        RuleFor(x => x.TransactionCode).NotEmpty().MaximumLength(32);
        RuleFor(x => x.ReceiverCustomerId).NotEmpty();
    }
}
