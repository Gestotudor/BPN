namespace BuildingBlocks.Responses;

/// <summary>
/// Standard error payload documented in Swagger for non-success responses.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Correlation identifier for tracing the failed request.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Validation or field-level errors keyed by field name.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}
