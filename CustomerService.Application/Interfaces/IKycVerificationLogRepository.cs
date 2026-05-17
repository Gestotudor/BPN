using CustomerService.Domain.Entities;

namespace CustomerService.Application.Interfaces;

public interface IKycVerificationLogRepository
{
    Task AddAsync(KycVerificationLog log, CancellationToken cancellationToken = default);
}
