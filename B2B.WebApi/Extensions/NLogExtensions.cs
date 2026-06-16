using NLog.Web;

namespace B2B.WebApi.Extensions;

public static class NLogExtensions
{
    public static ConfigureHostBuilder UseB2BNLog(this ConfigureHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog();
        return hostBuilder;
    }
}
