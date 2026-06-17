using B2B.WebApi.Middlewares;

namespace B2B.WebApi.Extensions;

/// <summary>
/// 提供 Web API 管線擴充方法。
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 加入安全性 HTTP 標頭。
    /// </summary>
    /// <param name="app">應用程式管線。</param>
    /// <returns>應用程式管線。</returns>
    public static IApplicationBuilder UseB2BSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
            context.Response.Headers.TryAdd("X-Frame-Options", "DENY");

            await next(context);
        });
    }

    /// <summary>
    /// 加入全域例外處理 middleware。
    /// </summary>
    /// <param name="app">應用程式管線。</param>
    /// <returns>應用程式管線。</returns>
    public static IApplicationBuilder UseB2BExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// 加入交易紀錄 middleware。
    /// </summary>
    /// <param name="app">應用程式管線。</param>
    /// <returns>應用程式管線。</returns>
    public static IApplicationBuilder UseB2BTransactionLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TransactionLogMiddleware>();
    }
}
