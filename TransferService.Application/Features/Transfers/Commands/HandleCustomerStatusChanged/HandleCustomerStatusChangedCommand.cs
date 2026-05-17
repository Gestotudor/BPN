using MediatR;
using TransferService.Application.Common.Results;

namespace TransferService.Application.Features.Transfers.Commands.HandleCustomerStatusChanged;

public sealed record HandleCustomerStatusChangedCommand(
    Guid CustomerId,
    string OldStatus,
    string NewStatus) : IRequest<Result<int>>;
