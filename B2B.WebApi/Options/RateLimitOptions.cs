using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Options;

/// <summary>
/// 表示 API 速率限制設定。
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// 速率限制設定區段名稱。
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// 取得或設定 Auth API 速率限制。
    /// </summary>
    public FixedWindowRateLimitOptions Auth { get; set; } = new();
}

/// <summary>
/// 表示固定時間窗速率限制設定。
/// </summary>
public sealed class FixedWindowRateLimitOptions
{
    /// <summary>
    /// 取得或設定時間窗內允許的請求數。
    /// </summary>
    [Range(1, 10_000)]
    public int PermitLimit { get; set; } = 5;

    /// <summary>
    /// 取得或設定時間窗秒數。
    /// </summary>
    [Range(1, 86_400)]
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// 取得或設定排隊請求數。
    /// </summary>
    [Range(0, 10_000)]
    public int QueueLimit { get; set; }
}
