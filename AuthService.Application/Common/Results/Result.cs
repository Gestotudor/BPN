namespace AuthService.Application.Common.Results;

public class Result
{
    protected Result(bool isSuccess, IReadOnlyCollection<string> errors, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        ErrorType = errorType;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<string> Errors { get; }

    public ResultErrorType ErrorType { get; }

    public static Result Success() => new(true, Array.Empty<string>(), ResultErrorType.None);

    public static Result Failure(ResultErrorType errorType, params string[] errors) =>
        new(false, errors, errorType);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IReadOnlyCollection<string> errors, ResultErrorType errorType)
        : base(isSuccess, errors, errorType)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) =>
        new(true, value, Array.Empty<string>(), ResultErrorType.None);

    public static new Result<T> Failure(ResultErrorType errorType, params string[] errors) =>
        new(false, default, errors, errorType);
}
