namespace Lms.Api.Common.Results;

/// <summary>
/// Represents the outcome of an operation without a return value,
/// following the Result pattern to avoid using exceptions for control flow.
/// </summary>
public readonly struct Result
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the error information when <see cref="IsSuccess"/> is <c>false</c>.</summary>
    public Error Error { get; }

    private Result(bool ok, Error error) { IsSuccess = ok; Error = error; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with a specific <see cref="Error"/>.</summary>
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The result value type.</typeparam>
public readonly struct Result<T>
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the result value when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    public T? Value { get; }

    /// <summary>Gets the error information when <see cref="IsSuccess"/> is <c>false</c>.</summary>
    public Error Error { get; }

    private Result(bool ok, T? value, Error error) { IsSuccess = ok; Value = value; Error = error; }

    /// <summary>Creates a successful result with <paramref name="value"/>.</summary>
    public static Result<T> Success(T value) => new(true, value, Error.None);

    /// <summary>Creates a failed result with a specific <see cref="Error"/>.</summary>
    public static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>Deconstructs the result into tuple-like components.</summary>
    public void Deconstruct(out bool ok, out T? value, out Error err) { ok = IsSuccess; value = Value; err = Error; }
}
