namespace B2B.Domain;

/// <summary>
/// 使用者清單查詢條件。
/// 未設定的欄位不會加入查詢條件。
/// </summary>
public sealed class UserFind
{
    /// <summary>
    /// 使用者識別碼；未設定時不限制識別碼。
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 帳號關鍵字；未設定時不限制帳號，設定後採不分大小寫部分符合。
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 顯示名稱關鍵字；未設定時不限制顯示名稱，設定後採不分大小寫部分符合。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 啟用狀態；未設定時不限制啟用狀態。
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 建立時間起點（含）；未設定時不限制起點。
    /// </summary>
    public DateTime? CreatedAtFrom { get; set; }

    /// <summary>
    /// 建立時間終點（含）；未設定時不限制終點。
    /// </summary>
    public DateTime? CreatedAtTo { get; set; }
}
