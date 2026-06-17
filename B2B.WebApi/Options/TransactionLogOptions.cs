using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Options;

/// <summary>
/// 表示交易紀錄 middleware 設定。
/// </summary>
public sealed class TransactionLogOptions
{
    /// <summary>
    /// 交易紀錄設定區段名稱。
    /// </summary>
    public const string SectionName = "TransactionLog";

    /// <summary>
    /// 取得或設定是否啟用交易紀錄。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 取得或設定是否記錄請求本文。
    /// </summary>
    public bool IncludeRequestBody { get; set; }

    /// <summary>
    /// 取得或設定是否記錄回應本文。
    /// </summary>
    public bool IncludeResponseBody { get; set; }

    /// <summary>
    /// 取得或設定是否信任反向代理提供的用戶端 IP 標頭。
    /// </summary>
    public bool TrustForwardedHeaders { get; set; }

    /// <summary>
    /// 取得或設定本文紀錄的最大長度。
    /// </summary>
    [Range(100, 1_000_000)]
    public int MaxBodyLogLength { get; set; } = 10000;
}
