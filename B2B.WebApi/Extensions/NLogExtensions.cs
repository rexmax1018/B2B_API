using NLog.Web;

namespace B2B.WebApi.Extensions;

/// <summary>
/// 提供 NLog 主機設定擴充方法。
/// </summary>
public static class NLogExtensions
{
    /// <summary>
    /// 啟用 B2B API 的 NLog 設定。
    /// </summary>
    /// <param name="hostBuilder">主機建構器。</param>
    /// <returns>主機建構器。</returns>
    public static ConfigureHostBuilder UseB2BNLog(this ConfigureHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog();
        return hostBuilder;
    }
}
