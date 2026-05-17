using MediatR;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Queries.GetTransferByCode;

public sealed record GetTransferByCodeQuery(string TransactionCode) : IRequest<Result<TransferResponse>>;
