using System.Text;
using System.Net;
using B2B.WebApi.Middlewares;
using B2B.WebApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace B2B.Tests;

/// <summary>
/// 驗證交易紀錄 middleware 的安全行為。
/// </summary>
public sealed class TransactionLogMiddlewareTests
{
    /// <summary>
    /// 驗證請求本文與查詢字串中的敏感資料會被遮蔽。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBodyLoggingEnabled_MasksSensitiveValues()
    {
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var options = new TestOptionsMonitor<TransactionLogOptions>(new TransactionLogOptions
        {
            Enabled = true,
            IncludeRequestBody = true,
            IncludeResponseBody = false,
            MaxBodyLogLength = 10000
        });
        var middleware = new TransactionLogMiddleware(
            _ => Task.CompletedTask,
            options,
            loggerFactory);
        var context = new DefaultHttpContext();
        var body = """
        {
          "account": "admin",
          "password": "PlainTextPassword",
          "profile": {
            "clientSecret": "ClientSecretValue"
          }
        }
        """;
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/auth/login";
        context.Request.QueryString = new QueryString("?apiKey=ApiKeyValue&token=TokenValue");
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = bodyBytes.Length;
        context.Request.Body = new MemoryStream(bodyBytes);

        await middleware.InvokeAsync(context);

        var logMessage = Assert.Single(provider.Messages);
        Assert.Contains("***", logMessage);
        Assert.DoesNotContain("PlainTextPassword", logMessage);
        Assert.DoesNotContain("ClientSecretValue", logMessage);
        Assert.DoesNotContain("ApiKeyValue", logMessage);
        Assert.DoesNotContain("TokenValue", logMessage);
    }

    /// <summary>
    /// 驗證交易紀錄不會直接信任未經 ForwardedHeaders middleware 處理的來源 IP 標頭。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenForwardedForHeaderExists_UsesConnectionRemoteIp()
    {
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var options = new TestOptionsMonitor<TransactionLogOptions>(new TransactionLogOptions
        {
            Enabled = true
        });
        var middleware = new TransactionLogMiddleware(
            _ => Task.CompletedTask,
            options,
            loggerFactory);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/auth/login";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.10";
        context.Connection.RemoteIpAddress = IPAddress.Loopback;

        await middleware.InvokeAsync(context);

        var logMessage = Assert.Single(provider.Messages);
        Assert.Contains(IPAddress.Loopback.ToString(), logMessage);
        Assert.DoesNotContain("203.0.113.10", logMessage);
    }

    private sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;

        public T Get(string? name) => value;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<string> Messages { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Messages);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<string> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                messages.Add(formatter(state, exception));
            }
        }
    }
}
