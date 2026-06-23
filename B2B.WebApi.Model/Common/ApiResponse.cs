namespace B2B.WebApi.Model.Common;

/// <summary>
/// 表示 API 的標準回應格式。
/// </summary>
/// <typeparam name="T">回應資料型別。</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// 取得或設定 API 是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 取得或設定成功時的資料。
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 取得或設定回應訊息。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 取得或設定失敗時的錯誤內容。
    /// </summary>
    public ErrorResponse? Error { get; set; }

    /// <summary>
    /// 取得或設定本次請求的追蹤編號。
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 建立成功回應。
    /// </summary>
    /// <param name="data">回應資料。</param>
    /// <param name="message">回應訊息。</param>
    /// <returns>成功回應。</returns>
    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        TraceId = traceId
    };

    /// <summary>
    /// 建立失敗回應。
    /// </summary>
    /// <param name="message">失敗訊息。</param>
    /// <param name="error">錯誤內容。</param>
    /// <returns>失敗回應。</returns>
    public static ApiResponse<T> Fail(string message, ErrorResponse error, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        Error = error,
        TraceId = traceId
    };
}
