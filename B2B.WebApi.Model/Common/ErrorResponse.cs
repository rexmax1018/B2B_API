namespace B2B.WebApi.Model.Common;

/// <summary>
/// 表示 API 錯誤內容。
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// 取得或設定錯誤代碼。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定錯誤訊息。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定補充錯誤內容。
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// 取得或設定欄位驗證錯誤。
    /// </summary>
    public IDictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// 初始化錯誤回應。
    /// </summary>
    public ErrorResponse()
    {
    }

    /// <summary>
    /// 使用錯誤代碼與訊息初始化錯誤回應。
    /// </summary>
    /// <param name="code">錯誤代碼。</param>
    /// <param name="message">錯誤訊息。</param>
    public ErrorResponse(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
