namespace B2B.Dao.Entities;

/// <summary>
/// 表示 B2B_USER 資料表的使用者資料。
/// </summary>
public sealed class UserEntity
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

    /// <summary>
    /// 取得或設定最後更新時間。
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
