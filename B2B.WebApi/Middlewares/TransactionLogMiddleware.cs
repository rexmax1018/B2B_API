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
        "authorization",
        "secret",
        "apiKey",
        "connectionString",
        "clientSecret"
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
        var sensitiveNames = BuildSensitiveNames(currentOptions);
        var requestBody = ShouldReadRequestBody(context, currentOptions)
            ? await ReadRequestBodyAsync(context, currentOptions, sensitiveNames)
            : null;

        var originalResponseBody = context.Response.Body;
        await using var responseBody = ShouldBufferResponseBody(context, currentOptions)
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
            var responseBodyText = responseBody is not null && IsAllowedContentType(context.Response.ContentType, currentOptions)
                ? await ReadResponseBodyAsync(context, currentOptions, sensitiveNames)
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
                查詢字串 = MaskText(context.Request.QueryString.Value ?? string.Empty, currentOptions.MaxBodyLogLength, sensitiveNames),
                狀態碼 = context.Response.StatusCode,
                請求本文 = requestBody,
                回應本文 = responseBodyText,
                用戶端IP = GetClientIp(context),
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
    private static async Task<string?> ReadRequestBodyAsync(
        HttpContext context,
        TransactionLogOptions options,
        IReadOnlySet<string> sensitiveNames)
    {
        if (context.Request.ContentLength > options.MaxBodyLogLength)
        {
            return $"本文長度 {context.Request.ContentLength} bytes 超過紀錄上限 {options.MaxBodyLogLength} bytes，已略過";
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        return MaskText(body, options.MaxBodyLogLength, sensitiveNames);
    }

    /// <summary>
    /// 讀取並遮罩回應本文。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="maxLength">最大紀錄長度。</param>
    /// <returns>遮罩後的回應本文。</returns>
    private static async Task<string?> ReadResponseBodyAsync(
        HttpContext context,
        TransactionLogOptions options,
        IReadOnlySet<string> sensitiveNames)
    {
        context.Response.Body.Position = 0;

        if (context.Response.Body.Length > options.MaxBodyLogLength)
        {
            context.Response.Body.Position = 0;
            return $"本文長度 {context.Response.Body.Length} bytes 超過紀錄上限 {options.MaxBodyLogLength} bytes，已略過";
        }

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Response.Body.Position = 0;

        return MaskText(body, options.MaxBodyLogLength, sensitiveNames);
    }

    /// <summary>
    /// 遮罩敏感文字並限制紀錄長度。
    /// </summary>
    /// <param name="text">來源文字。</param>
    /// <param name="maxLength">最大紀錄長度。</param>
    /// <returns>遮罩後的文字。</returns>
    private static string? MaskText(
        string? text,
        int maxLength,
        IReadOnlySet<string> sensitiveNames)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var masked = TryMaskJson(text, sensitiveNames) ?? SensitiveQueryRegex().Replace(text, "$1=***");

        return masked.Length <= maxLength
            ? masked
            : $"{masked[..maxLength]}...已截斷";
    }

    /// <summary>
    /// 嘗試以 JSON 格式解析並遮罩敏感欄位。
    /// </summary>
    /// <param name="text">來源文字。</param>
    /// <returns>遮罩後的 JSON；解析失敗時為 <see langword="null"/>。</returns>
    private static string? TryMaskJson(string text, IReadOnlySet<string> sensitiveNames)
    {
        try
        {
            var node = JsonNode.Parse(text);

            if (node is null)
            {
                return null;
            }

            MaskJsonNode(node, sensitiveNames);

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
    private static void MaskJsonNode(JsonNode node, IReadOnlySet<string> sensitiveNames)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (IsSensitiveName(property.Key, sensitiveNames))
                {
                    jsonObject[property.Key] = "***";
                    continue;
                }

                if (property.Value is not null)
                {
                    MaskJsonNode(property.Value, sensitiveNames);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    MaskJsonNode(item, sensitiveNames);
                }
            }
        }
    }

    /// <summary>
    /// 判斷是否可讀取請求本文。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>可讀取時為 <see langword="true"/>。</returns>
    private static bool ShouldReadRequestBody(HttpContext context, TransactionLogOptions options)
    {
        return options.IncludeRequestBody &&
            !IsExcludedPath(context, options) &&
            context.Request.ContentLength is > 0 &&
            IsAllowedContentType(context.Request.ContentType, options);
    }

    /// <summary>
    /// 判斷是否可緩衝回應本文。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>可緩衝時為 <see langword="true"/>。</returns>
    private static bool ShouldBufferResponseBody(HttpContext context, TransactionLogOptions options)
    {
        return options.IncludeResponseBody &&
            !IsExcludedPath(context, options) &&
            RequestAcceptsTextResponse(context);
    }

    /// <summary>
    /// 判斷是否為允許記錄本文的 Content-Type。
    /// </summary>
    /// <param name="contentType">Content-Type。</param>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>允許時為 <see langword="true"/>。</returns>
    private static bool IsAllowedContentType(string? contentType, TransactionLogOptions options)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return options.AllowedBodyContentTypes.Any(allowed =>
            contentType.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判斷路徑是否排除本文紀錄。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>排除時為 <see langword="true"/>。</returns>
    private static bool IsExcludedPath(HttpContext context, TransactionLogOptions options)
    {
        var path = context.Request.Path.Value;

        return !string.IsNullOrWhiteSpace(path) &&
            options.ExcludedBodyPathPrefixes.Any(prefix =>
                path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判斷用戶端是否偏好文字型回應。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <returns>偏好文字型回應時為 <see langword="true"/>。</returns>
    private static bool RequestAcceptsTextResponse(HttpContext context)
    {
        var acceptHeaders = context.Request.Headers.Accept;

        return acceptHeaders.Count == 0 ||
            acceptHeaders.Any(value =>
                !string.IsNullOrWhiteSpace(value) &&
                (value.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("text", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("*/*", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// 建立敏感欄位名稱集合。
    /// </summary>
    /// <param name="options">交易紀錄選項。</param>
    /// <returns>敏感欄位名稱集合。</returns>
    private static IReadOnlySet<string> BuildSensitiveNames(TransactionLogOptions options)
    {
        var names = new HashSet<string>(SensitiveNames, StringComparer.OrdinalIgnoreCase);

        foreach (var name in options.SensitiveFieldNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

    /// <summary>
    /// 判斷欄位名稱是否敏感。
    /// </summary>
    /// <param name="name">欄位名稱。</param>
    /// <param name="sensitiveNames">敏感欄位名稱集合。</param>
    /// <returns>敏感時為 <see langword="true"/>。</returns>
    private static bool IsSensitiveName(string name, IReadOnlySet<string> sensitiveNames)
    {
        return sensitiveNames.Any(sensitiveName =>
            name.Contains(sensitiveName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 取得用戶端 IP。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <returns>用戶端 IP。</returns>
    private static string? GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// 取得遮罩查詢字串敏感值的正規表示式。
    /// </summary>
    /// <returns>敏感查詢字串正規表示式。</returns>
    [GeneratedRegex(@"(?i)(password|accessToken|refreshToken|token|authorization|secret|apiKey|connectionString|clientSecret)=([^&\s]+)")]
    private static partial Regex SensitiveQueryRegex();
}
