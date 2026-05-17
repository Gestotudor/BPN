using MediatR;
using TransferService.Application.Common.Mappings;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;

namespace TransferService.Application.Features.Transfers.Queries.GetTransferByCode;

public sealed class GetTransferByCodeQueryHandler : IRequestHandler<GetTransferByCodeQuery, Result<TransferResponse>>
{
    private readonly ITransferRepository _transferRepository;

    public GetTransferByCodeQueryHandler(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public async Task<Result<TransferResponse>> Handle(GetTransferByCodeQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _transferRepository.GetByTransactionCodeAsync(request.TransactionCode, cancellationToken);
        return transfer is null
            ? Result<TransferResponse>.Failure(ResultErrorType.NotFound, "Transfer not found.")
            : Result<TransferResponse>.Success(transfer.ToResponse());
    }
}
