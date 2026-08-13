namespace B2B.Domain;

/// <summary>
/// 表示資料庫中的 Refresh Token 狀態。
/// </summary>
public sealed class RefreshTokenDomain
{
    /// <summary>
    /// 取得或設定 Refresh Token 識別碼。
    /// </summary>
    public long RefreshTokenId { get; set; }

    /// <summary>
    /// 取得或設定權杖所屬的服務識別碼。
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定 Refresh Token 值。
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定權杖到期時間。
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 取得或設定權杖建立時間。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 取得或設定建立權杖的用戶端 IP。
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// 取得或設定權杖撤銷時間。
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// 取得或設定撤銷權杖的用戶端 IP。
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// 取得或設定取代此權杖的新權杖。
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// 取得或設定權杖是否已撤銷。
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// 取得權杖是否已到期。
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// 取得權杖是否仍可使用。
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;
}
