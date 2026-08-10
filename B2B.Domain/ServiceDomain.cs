namespace B2B.Domain;

/// <summary>
/// 表示以 Entry 憑證驗證的服務身分。
/// </summary>
public sealed class ServiceDomain
{
    /// <summary>
    /// 取得或設定服務識別碼。
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定服務名稱。
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
}
