namespace B2B.WebApi.Model.Common;

public sealed class ErrorResponse
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public ErrorResponse()
    {
    }

    public ErrorResponse(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
