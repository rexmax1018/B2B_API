using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Options;

/// <summary>
/// 表示反向代理 Forwarded Headers 設定。
/// </summary>
public sealed class B2BForwardedHeadersOptions
{
    /// <summary>
    /// Forwarded Headers 設定區段名稱。
    /// </summary>
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    /// 取得或設定是否啟用 Forwarded Headers middleware。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 取得或設定允許處理的 Forwarded Headers 層數。
    /// </summary>
    [Range(1, 10)]
    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// 取得或設定可信任的代理伺服器 IP。
    /// </summary>
    public string[] KnownProxies { get; set; } = [];
}
