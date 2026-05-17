using CustomerService.Application.Features.Customers.DTOs;
using CustomerService.Domain.Entities;

namespace CustomerService.Application.Common.Mappings;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Surname,
            $"{customer.Name} {customer.Surname}",
            customer.NationalIdNumber,
            customer.TaxNumber,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.Type,
            customer.Status,
            customer.IsKycVerified,
            customer.KycVerifiedAt,
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}
