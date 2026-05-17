namespace TransferService.Application.Common.Results;

public enum ResultErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    ServiceUnavailable = 5
}
