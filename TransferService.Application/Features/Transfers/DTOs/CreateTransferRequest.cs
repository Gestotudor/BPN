namespace TransferService.Application.Features.Transfers.DTOs;

public sealed record CreateTransferRequest(
    TransferCustomerRequest SenderCustomer,
    TransferCustomerRequest ReceiverCustomer,
    decimal Amount,
    string Currency);

public sealed record TransferCustomerRequest(
    string Name,
    string Surname,
    string NationalIdNumber,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    TransferCustomerType Type);

public enum TransferCustomerType
{
    Individual = 1,
    Corporate = 2
}
