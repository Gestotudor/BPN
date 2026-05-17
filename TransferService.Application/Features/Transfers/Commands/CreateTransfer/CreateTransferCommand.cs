using MediatR;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Commands.CreateTransfer;

public sealed record CreateTransferCommand(
    TransferCustomerRequest SenderCustomer,
    TransferCustomerRequest ReceiverCustomer,
    decimal Amount,
    string Currency,
    string IdempotencyKey) : IRequest<Result<TransferResponse>>;
