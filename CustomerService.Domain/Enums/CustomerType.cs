namespace CustomerService.Domain.Enums;

/// <summary>
/// Type of customer registered in MoneyBee.
/// </summary>
public enum CustomerType
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
