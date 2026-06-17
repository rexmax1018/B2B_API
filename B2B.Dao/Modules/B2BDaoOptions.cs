namespace B2B.Dao.Modules;

/// <summary>
/// 表示資料存取層連線設定。
/// </summary>
public sealed class B2BDaoOptions
{
    /// <summary>
    /// 取得資料庫連線字串。
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;
}
