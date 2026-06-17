using System.Net;
using System.Text.Json;
using B2B.WebApi.Model.Common;

namespace B2B.WebApi.Middlewares;

/// <summary>
/// 捕捉未處理例外並輸出標準 API 錯誤回應。
/// </summary>
/// <param name="next">下一個 middleware。</param>
/// <param name="logger">例外記錄器。</param>
/// <param name="environment">主機環境。</param>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// 執行全域例外處理流程。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
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
                "發生未處理例外。追蹤編號：{TraceId}，路徑：{Path}，環境：{Environment}",
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
