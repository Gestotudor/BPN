using MediatR;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;

namespace TransferService.Application.Features.Transfers.Queries.GetTransferById;

public sealed record GetTransferByIdQuery(Guid TransferId) : IRequest<Result<TransferResponse>>;
