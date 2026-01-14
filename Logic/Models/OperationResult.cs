namespace Logic.Models;

public class OperationResult<T>
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public T? Value { get; init; }

    public static OperationResult<T> Ok(T value)
    {
        return new OperationResult<T> { Success = true, Value = value };
    }

    public static OperationResult<T> Fail(string error)
    {
        return new OperationResult<T> { Success = false, Error = error };
    }
}
