using CustomerService.Application.Common.Mappings;
using CustomerService.Application.Common.Results;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Features.Customers.Commands.ChangeCustomerStatus;

public sealed class ChangeCustomerStatusCommandHandler
    : IRequestHandler<ChangeCustomerStatusCommand, Result<DTOs.CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerStatusHistoryRepository _customerStatusHistoryRepository;
    private readonly ITransferServiceClient _transferServiceClient;
    private readonly ILogger<ChangeCustomerStatusCommandHandler> _logger;

    public ChangeCustomerStatusCommandHandler(
        ICustomerRepository customerRepository,
        ICustomerStatusHistoryRepository customerStatusHistoryRepository,
        ITransferServiceClient transferServiceClient,
        ILogger<ChangeCustomerStatusCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _customerStatusHistoryRepository = customerStatusHistoryRepository;
        _transferServiceClient = transferServiceClient;
        _logger = logger;
    }

    public async Task<Result<DTOs.CustomerResponse>> Handle(
        ChangeCustomerStatusCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<DTOs.CustomerResponse>.Failure(ResultErrorType.NotFound, "Customer not found.");
        }

        var oldStatus = customer.Status;
        customer.Status = request.Status;
        customer.UpdatedAt = DateTime.UtcNow;

        await _customerStatusHistoryRepository.AddAsync(
            new CustomerStatusHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Customer = customer,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                ChangedAt = DateTime.UtcNow
            },
            cancellationToken);

        await _customerRepository.SaveChangesAsync(cancellationToken);

        await _transferServiceClient.NotifyCustomerStatusChangedAsync(
            customer.Id,
            oldStatus,
            request.Status,
            cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} status changed from {OldStatus} to {NewStatus}",
            customer.Id,
            oldStatus,
            request.Status);

        return Result<DTOs.CustomerResponse>.Success(customer.ToResponse());
    }
}
