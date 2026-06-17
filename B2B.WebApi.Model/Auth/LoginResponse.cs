namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登入成功後的權杖回應。
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// 取得或設定 Access Token。
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定 Refresh Token。
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定權杖類型。
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 取得或設定 Access Token 有效秒數。
    /// </summary>
    public int ExpiresIn { get; set; }
}
