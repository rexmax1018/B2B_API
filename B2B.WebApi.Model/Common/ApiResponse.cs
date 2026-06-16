namespace B2B.WebApi.Model.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public string? Message { get; set; }

    public ErrorResponse? Error { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message, ErrorResponse error) => new()
    {
        Success = false,
        Message = message,
        Error = error
    };
}
