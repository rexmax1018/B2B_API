using System.Net;
using System.Text.Json;
using B2B.WebApi.Model.Common;

namespace B2B.WebApi.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Environment: {Environment}",
                context.TraceIdentifier,
                context.Request.Path,
                environment.EnvironmentName);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = ApiResponse<object>.Fail(
                "系統發生錯誤",
                new ErrorResponse("SYS_ERROR", "系統發生錯誤"));

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, JsonSerializerOptions),
                context.RequestAborted);
        }
    }
}
