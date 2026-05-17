namespace CustomerService.Application.Common.Results;

public enum ResultErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    ServiceUnavailable = 6,
    Failure = 7
}
