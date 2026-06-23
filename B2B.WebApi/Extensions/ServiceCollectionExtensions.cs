using System.Net;
using System.Text.Json;
using B2B.Service.Options;
using B2B.WebApi.Filters;
using B2B.WebApi.HealthChecks;
using B2B.WebApi.Model.Common;
using B2B.WebApi.Options;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

namespace B2B.WebApi.Extensions;

/// <summary>
/// 提供 B2B API 服務註冊擴充方法。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 加入選項設定與記憶體快取。
    /// </summary>
    /// <param name="services">服務集合。</param>
    /// <param name="configuration">應用程式設定。</param>
    /// <returns>服務集合。</returns>
    public static IServiceCollection AddB2BOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<TransactionLogOptions>()
            .Bind(configuration.GetSection(TransactionLogOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<B2BForwardedHeadersOptions>()
            .Bind(configuration.GetSection(B2BForwardedHeadersOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            var forwardedHeadersOptions = configuration
                .GetSection(B2BForwardedHeadersOptions.SectionName)
                .Get<B2BForwardedHeadersOptions>() ?? new B2BForwardedHeadersOptions();

            if (!forwardedHeadersOptions.Enabled)
            {
                return;
            }

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = forwardedHeadersOptions.ForwardLimit;

            foreach (var knownProxy in forwardedHeadersOptions.KnownProxies)
            {
                if (IPAddress.TryParse(knownProxy, out var ipAddress))
                {
                    options.KnownProxies.Add(ipAddress);
                }
            }
        });

        services.AddMemoryCache();
        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("OK"),
                tags: ["live"])
            .AddCheck<OracleHealthCheck>(
                "oracle",
                tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// 加入 API 速率限制設定。
    /// </summary>
    /// <param name="services">服務集合。</param>
    /// <returns>服務集合。</returns>
    public static IServiceCollection AddB2BRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitOptions = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

                var response = ApiResponse<object>.Fail(
                    "請求過於頻繁，請稍後再試",
                    new ErrorResponse("RATE_LIMITED", "請求過於頻繁，請稍後再試"),
                    context.HttpContext.TraceIdentifier);

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                    cancellationToken);
            };

            options.AddPolicy("Auth", httpContext =>
            {
                var partitionKey = BuildAuthRateLimitPartitionKey(httpContext);
                var authOptions = rateLimitOptions.Auth;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authOptions.PermitLimit,
                        Window = TimeSpan.FromSeconds(authOptions.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = authOptions.QueueLimit
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// 加入控制器、端點探索與 Swagger 文件設定。
    /// </summary>
    /// <param name="services">服務集合。</param>
    /// <returns>服務集合。</returns>
    public static IServiceCollection AddB2BSwagger(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseTraceIdFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var validationErrors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(error =>
                            string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? $"{x.Key} 欄位格式不正確"
                                : error.ErrorMessage).ToArray())
                        .Where(x => x.Value.Length > 0)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var errors = validationErrors
                        .SelectMany(x => x.Value)
                        .ToArray();

                    var message = errors.Length == 0
                        ? "請求驗證失敗"
                        : string.Join("；", errors);

                    return new BadRequestObjectResult(ApiResponse<object>.Fail(
                        "請求驗證失敗",
                        new ErrorResponse("VALIDATION_FAILED", message)
                        {
                            ValidationErrors = validationErrors
                        },
                        context.HttpContext.TraceIdentifier));
                };
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "B2B_API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "請輸入 JWT Bearer Token"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    []
                }
            });
        });

        return services;
    }

    /// <summary>
    /// 建立 Auth API 分區限流鍵。
    /// </summary>
    /// <param name="context">HTTP 內容。</param>
    /// <returns>限流分區鍵。</returns>
    private static string BuildAuthRateLimitPartitionKey(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var clientKey = string.IsNullOrWhiteSpace(remoteIp)
            ? "unknown-client"
            : remoteIp;

        return string.IsNullOrWhiteSpace(userAgent)
            ? clientKey
            : $"{clientKey}:{userAgent}";
    }
}
