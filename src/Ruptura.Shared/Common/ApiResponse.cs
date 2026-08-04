namespace Ruptura.Shared.Common;

public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? Message = null,
    string[]? Errors = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(true, data, message);

    public static ApiResponse<T> Fail(string message, string[]? errors = null) =>
        new(false, default, message, errors);
}

public record ApiResponse(
    bool Success,
    string? Message = null,
    string[]? Errors = null)
{
    public static ApiResponse Ok(string? message = null) => new(true, message);
    public static ApiResponse Fail(string message, string[]? errors = null) =>
        new(false, message, errors);
}
