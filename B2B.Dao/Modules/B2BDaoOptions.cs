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

    /// <summary>
    /// 取得或設定 B2B.Conn 環境別。
    /// </summary>
    public string EnvType { get; init; } = string.Empty;

    /// <summary>
    /// 取得或設定 B2B.Conn 服務類型。
    /// </summary>
    public string SvrType { get; init; } = string.Empty;

    /// <summary>
    /// 取得或設定 B2B.Conn 資料庫類型。
    /// </summary>
    public string DBType { get; init; } = string.Empty;

    /// <summary>
    /// 取得或設定 B2B.Conn 帳號類型。
    /// </summary>
    public string AccType { get; init; } = string.Empty;
}
