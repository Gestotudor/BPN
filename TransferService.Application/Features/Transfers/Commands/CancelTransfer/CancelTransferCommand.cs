using MediatR;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Commands.CancelTransfer;

public sealed record CancelTransferCommand(Guid TransferId, string? Reason) : IRequest<Result<TransferResponse>>;
