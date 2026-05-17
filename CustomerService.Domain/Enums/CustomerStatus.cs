namespace CustomerService.Domain.Enums;

/// <summary>
/// Operational status of a customer in MoneyBee.
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Customer can send and receive transfers.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Customer exists but cannot transact.
    /// </summary>
    Passive = 2,

    /// <summary>
    /// Customer is blocked and pending transfers must be cancelled.
    /// </summary>
    Blocked = 3
}
