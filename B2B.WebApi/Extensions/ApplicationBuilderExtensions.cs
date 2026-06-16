using B2B.WebApi.Middlewares;

namespace B2B.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseB2BExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseB2BTransactionLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TransactionLogMiddleware>();
    }
}
