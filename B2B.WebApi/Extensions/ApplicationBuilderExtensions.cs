using B2B.WebApi.Middlewares;

namespace B2B.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
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

    public static IApplicationBuilder UseB2BExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseB2BTransactionLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TransactionLogMiddleware>();
    }
}
