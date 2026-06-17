namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示更新權杖成功後的回應內容。
/// </summary>
public sealed class RefreshTokenResponse
{
    /// <summary>
    /// 取得或設定新的 Access Token。
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定新的 Refresh Token。
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
