namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登入 API 的請求內容。
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// 取得或設定登入帳號。
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 取得或設定登入密碼。
    /// </summary>
    public string? Password { get; set; }
}
