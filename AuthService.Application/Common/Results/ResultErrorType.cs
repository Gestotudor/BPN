namespace AuthService.Application.Common.Results;

public enum ResultErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Conflict = 4,
    Failure = 5
}
