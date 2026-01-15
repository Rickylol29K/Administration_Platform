namespace Logic.Models;

public class OperationResult<T>
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public T? Value { get; private set; }

    private OperationResult(bool success, T? value, string? error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    public static OperationResult<T> Ok(T value)
    {
        return new OperationResult<T>(true, value, null);
    }

    public static OperationResult<T> Fail(string error)
    {
        return new OperationResult<T>(false, default, error);
    }
}
