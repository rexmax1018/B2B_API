namespace B2B.Domain;

/// <summary>
/// 表示登入後簽發的 Access Token 與 Refresh Token。
/// </summary>
public sealed class TokenDomain
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

    /// <summary>
    /// 取得或設定 Access Token 到期時間。
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// 取得或設定 Refresh Token 到期時間。
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}
