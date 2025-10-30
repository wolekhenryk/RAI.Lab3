namespace RAI.Lab3.Infrastructure;

public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null) throw new ArgumentException("Success cannot have an error.");
        if (!isSuccess && error is null) throw new ArgumentNullException(nameof(error));
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    // convenience if you still pass strings sometimes
    public static Result Failure(string message) => Failure(new Error("error", message));

    public override string ToString() => IsSuccess ? "Success" : $"Failure({Error})";
}

/// <summary>Represents a result of an operation that can either be a success or a failure.</summary>
public sealed class Result<TResult>
{
    private readonly TResult _value = default!;
    private readonly Error _error = default!;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(TResult value, Error error, bool isSuccess)
    {
        _value = value;
        _error = error;
        IsSuccess = isSuccess;
    }

    public static Result<TResult> Success(TResult value)
        => value is null
            ? throw new ArgumentNullException(nameof(value), "Success value cannot be null")
            : new Result<TResult>(value, default!, true);

    public static Result<TResult> Failure(Error error)
        => error is null
            ? throw new ArgumentNullException(nameof(error), "Error cannot be null")
            : new Result<TResult>(default!, error, false);

    // convenience if you still pass strings sometimes
    public static Result<TResult> Failure(string message)
        => Failure(new Error("error", message));

    public TResult Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot get value from a failed result");

    public Error Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot get error from a successful result");

    public override string ToString() => IsSuccess ? $"Success({_value})" : $"Failure({_error})";
}
