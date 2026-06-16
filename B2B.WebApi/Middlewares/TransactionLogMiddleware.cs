using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using B2B.WebApi.Options;
using Microsoft.Extensions.Options;

namespace B2B.WebApi.Middlewares;

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
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var responseTime = DateTime.UtcNow;
            var responseBodyText = currentOptions.IncludeResponseBody
                ? await ReadResponseBodyAsync(context, currentOptions.MaxBodyLogLength)
                : null;

            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody, context.RequestAborted);
            context.Response.Body = originalResponseBody;

            var payload = new
            {
                context.TraceIdentifier,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = MaskText(context.Request.QueryString.Value ?? string.Empty, currentOptions.MaxBodyLogLength),
                context.Response.StatusCode,
                RequestBody = requestBody,
                ResponseBody = responseBodyText,
                ClientIp = GetClientIp(context),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                RequestTime = requestTime,
                ResponseTime = responseTime
            };

            transactionLogger.LogInformation("{TransactionLog}", JsonSerializer.Serialize(payload));
        }
    }

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

    private static string? MaskText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var masked = TryMaskJson(text) ?? SensitiveQueryRegex().Replace(text, "$1=***");

        return masked.Length <= maxLength
            ? masked
            : $"{masked[..maxLength]}...TRUNCATED";
    }

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

    private static string? GetClientIp(HttpContext context)
    {
        return GetFirstHeaderValue(context, "X-Forwarded-For")
            ?? GetFirstHeaderValue(context, "X-Real-IP")
            ?? context.Connection.RemoteIpAddress?.ToString();
    }

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

    [GeneratedRegex(@"(?i)(password|accessToken|refreshToken|token|authorization)=([^&\s]+)")]
    private static partial Regex SensitiveQueryRegex();
}
