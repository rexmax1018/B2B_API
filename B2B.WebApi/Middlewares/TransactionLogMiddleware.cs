using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using B2B.WebApi.Options;
using Microsoft.Extensions.Options;

namespace B2B.WebApi.Middlewares;

/// <summary>
/// 記錄 HTTP 請求與回應的交易紀錄。
/// </summary>
/// <param name="next">下一個 middleware。</param>
/// <param name="options">交易紀錄選項。</param>
/// <param name="loggerFactory">記錄器工廠。</param>
public sealed partial class TransactionLogMiddleware(
    RequestDelegate next,
    IOptionsMonitor<TransactionLogOptions> options,
    ILoggerFactory loggerFactory)
{
    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "accessToken",
        "refreshToken",
        "token",
        "authorization"
    };

    private readonly ILogger transactionLogger = loggerFactory.CreateLogger("TransactionLogger");

    /// <summary>
    /// 執行交易紀錄流程。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var currentOptions = options.CurrentValue;

        if (!currentOptions.Enabled)
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestTime = DateTime.UtcNow;
        var requestBody = currentOptions.IncludeRequestBody
            ? await ReadRequestBodyAsync(context, currentOptions.MaxBodyLogLength)
            : null;

        var originalResponseBody = context.Response.Body;
        await using var responseBody = currentOptions.IncludeResponseBody
            ? new MemoryStream()
            : null;

        if (responseBody is not null)
        {
            context.Response.Body = responseBody;
        }

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var responseTime = DateTime.UtcNow;
            var responseBodyText = responseBody is not null
                ? await ReadResponseBodyAsync(context, currentOptions.MaxBodyLogLength)
                : null;

            if (responseBody is not null)
            {
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalResponseBody, context.RequestAborted);
                context.Response.Body = originalResponseBody;
            }

            var payload = new
            {
                追蹤編號 = context.TraceIdentifier,
                HTTP方法 = context.Request.Method,
                路徑 = context.Request.Path.Value,
                查詢字串 = MaskText(context.Request.QueryString.Value ?? string.Empty, currentOptions.MaxBodyLogLength),
                狀態碼 = context.Response.StatusCode,
                請求本文 = requestBody,
                回應本文 = responseBodyText,
                用戶端IP = GetClientIp(context, currentOptions),
                使用者代理 = context.Request.Headers.UserAgent.ToString(),
                耗時毫秒 = stopwatch.ElapsedMilliseconds,
                請求時間 = requestTime,
                回應時間 = responseTime
            };

            transactionLogger.LogInformation("交易紀錄：{TransactionLog}", JsonSerializer.Serialize(payload));
        }
    }

    /// <summary>
    /// 讀取並遮罩請求本文。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="maxLength">最大紀錄長度。</param>
    /// <returns>遮罩後的請求本文。</returns>
    private static async Task<string?> ReadRequestBodyAsync(HttpContext context, int maxLength)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        return MaskText(body, maxLength);
    }

    /// <summary>
    /// 讀取並遮罩回應本文。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="maxLength">最大紀錄長度。</param>
    /// <returns>遮罩後的回應本文。</returns>
    private static async Task<string?> ReadResponseBodyAsync(HttpContext context, int maxLength)
    {
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Response.Body.Position = 0;

        return MaskText(body, maxLength);
    }

    /// <summary>
    /// 遮罩敏感文字並限制紀錄長度。
    /// </summary>
    /// <param name="text">來源文字。</param>
    /// <param name="maxLength">最大紀錄長度。</param>
    /// <returns>遮罩後的文字。</returns>
    private static string? MaskText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var masked = TryMaskJson(text) ?? SensitiveQueryRegex().Replace(text, "$1=***");

        return masked.Length <= maxLength
            ? masked
            : $"{masked[..maxLength]}...已截斷";
    }

    /// <summary>
    /// 嘗試以 JSON 格式解析並遮罩敏感欄位。
    /// </summary>
    /// <param name="text">來源文字。</param>
    /// <returns>遮罩後的 JSON；解析失敗時為 <see langword="null"/>。</returns>
    private static string? TryMaskJson(string text)
    {
        try
        {
            var node = JsonNode.Parse(text);

            if (node is null)
            {
                return null;
            }

            MaskJsonNode(node);

            return node.ToJsonString();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// 遞迴遮罩 JSON 節點中的敏感欄位。
    /// </summary>
    /// <param name="node">JSON 節點。</param>
    private static void MaskJsonNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                    SensitiveNames.Contains(property.Key))
                {
                    jsonObject[property.Key] = "***";
                    continue;
                }

                if (property.Value is not null)
                {
                    MaskJsonNode(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    MaskJsonNode(item);
                }
            }
        }
    }

    /// <summary>
    /// 取得用戶端 IP。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>用戶端 IP。</returns>
    private static string? GetClientIp(HttpContext context, TransactionLogOptions options)
    {
        if (options.TrustForwardedHeaders)
        {
            return GetFirstHeaderValue(context, "X-Forwarded-For")
                ?? GetFirstHeaderValue(context, "X-Real-IP")
                ?? context.Connection.RemoteIpAddress?.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// 取得指定標頭的第一個有效值。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="headerName">標頭名稱。</param>
    /// <returns>第一個有效標頭值。</returns>
    private static string? GetFirstHeaderValue(HttpContext context, string headerName)
    {
        if (!context.Request.Headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var firstValue = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(firstValue) &&
                !string.Equals(firstValue, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                return firstValue;
            }
        }

        return null;
    }

    /// <summary>
    /// 取得遮罩查詢字串敏感值的正規表示式。
    /// </summary>
    /// <returns>敏感查詢字串正規表示式。</returns>
    [GeneratedRegex(@"(?i)(password|accessToken|refreshToken|token|authorization)=([^&\s]+)")]
    private static partial Regex SensitiveQueryRegex();
}
