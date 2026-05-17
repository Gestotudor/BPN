using MediatR;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Commands.ReceiveTransfer;

public sealed record ReceiveTransferCommand(
    string TransactionCode,
    Guid ReceiverCustomerId) : IRequest<Result<TransferResponse>>;
