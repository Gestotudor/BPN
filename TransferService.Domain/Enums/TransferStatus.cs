namespace TransferService.Domain.Enums;

/// <summary>
/// Lifecycle status of a money transfer.
/// </summary>
public enum TransferStatus
{
    /// <summary>
    /// Transfer created but not yet received.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Receiver received the money.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Transfer cancelled before receiving.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Transfer rejected or failed.
    /// </summary>
    Failed = 4
}
