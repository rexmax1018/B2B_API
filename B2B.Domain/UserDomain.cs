namespace B2B.Domain;

/// <summary>
/// 表示系統使用者的領域資料。
/// </summary>
public sealed class UserDomain
{
    /// <summary>
    /// 取得或設定使用者識別碼。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 取得或設定登入帳號。
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定顯示名稱。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定密碼雜湊值。
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定使用者是否啟用。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 取得或設定建立時間。
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
