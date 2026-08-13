namespace B2B.Domain.Models;

/// <summary>
/// 表示快取中的 Refresh Token 資料。
/// </summary>
public class RefreshTokenModel
{
    /// <summary>
    /// 取得或設定權杖所屬的服務識別碼。
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定權杖所屬的服務名稱。
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定權杖建立時間。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 取得或設定權杖到期時間。
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 取得或設定權杖是否已撤銷。
    /// </summary>
    public bool IsRevoked { get; set; }
}
