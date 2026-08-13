namespace B2B.Domain;

/// <summary>
/// 表示登入、更新權杖或登出的服務處理結果。
/// </summary>
public sealed class LoginResultDomain
{
    /// <summary>
    /// 取得或設定處理是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 取得或設定回傳給呼叫端的結果訊息。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 取得或設定失敗時的錯誤代碼。
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 取得或設定成功簽發的權杖資料。
    /// </summary>
    public TokenDomain? Token { get; set; }

    /// <summary>
    /// 建立失敗的登入結果。
    /// </summary>
    /// <param name="message">失敗訊息。</param>
    /// <param name="errorCode">錯誤代碼。</param>
    /// <returns>失敗結果。</returns>
    public static LoginResultDomain Failed(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };

    /// <summary>
    /// 建立成功的登入結果。
    /// </summary>
    /// <param name="token">權杖資料。</param>
    /// <returns>成功結果。</returns>
    public static LoginResultDomain Succeeded(TokenDomain token) => new()
    {
        Success = true,
        Token = token
    };
}
