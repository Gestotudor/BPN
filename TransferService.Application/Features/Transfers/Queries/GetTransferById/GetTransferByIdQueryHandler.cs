using MediatR;
using TransferService.Application.Common.Mappings;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;

namespace TransferService.Application.Features.Transfers.Queries.GetTransferById;

public sealed class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, Result<TransferResponse>>
{
    private readonly ITransferRepository _transferRepository;

    public GetTransferByIdQueryHandler(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public async Task<Result<TransferResponse>> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _transferRepository.GetByIdAsync(request.TransferId, cancellationToken);
        return transfer is null
            ? Result<TransferResponse>.Failure(ResultErrorType.NotFound, "Transfer not found.")
            : Result<TransferResponse>.Success(transfer.ToResponse());
    }
}
