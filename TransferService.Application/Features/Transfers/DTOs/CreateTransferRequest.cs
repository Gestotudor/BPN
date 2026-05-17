namespace TransferService.Application.Features.Transfers.DTOs;

/// <summary>
/// Request payload used to create a new money transfer.
/// </summary>
/// <param name="SenderCustomer">Sender customer details used for validation.</param>
/// <param name="ReceiverCustomer">Receiver customer details used for validation.</param>
/// <param name="Amount">Transfer amount in the requested currency.</param>
/// <param name="Currency">ISO currency code for the transfer amount.</param>
public sealed record CreateTransferRequest(
    TransferCustomerRequest SenderCustomer,
    TransferCustomerRequest ReceiverCustomer,
    decimal Amount,
    string Currency);

/// <summary>
/// Customer information supplied as part of transfer creation.
/// </summary>
/// <param name="Name">Customer first name.</param>
/// <param name="Surname">Customer surname.</param>
/// <param name="NationalIdNumber">Customer national identity number.</param>
/// <param name="TaxNumber">Customer tax number for corporate customers.</param>
/// <param name="PhoneNumber">Customer phone number.</param>
/// <param name="DateOfBirth">Customer date of birth.</param>
/// <param name="Type">Customer type.</param>
public sealed record TransferCustomerRequest(
    string Name,
    string Surname,
    string NationalIdNumber,
    string? TaxNumber,
    string PhoneNumber,
    DateTime DateOfBirth,
    TransferCustomerType Type);

/// <summary>
/// Type of customer supplied within a transfer request.
/// </summary>
public enum TransferCustomerType
{
    /// <summary>
    /// Real person customer.
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Company customer. Tax number is required.
    /// </summary>
    Corporate = 2
}
