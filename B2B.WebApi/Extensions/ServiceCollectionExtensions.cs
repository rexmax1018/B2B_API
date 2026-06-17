using B2B.WebApi.Options;
using Microsoft.AspNetCore.RateLimiting;
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

        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// 加入 API 速率限制設定。
    /// </summary>
    /// <param name="services">服務集合。</param>
    /// <returns>服務集合。</returns>
    public static IServiceCollection AddB2BRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
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
        services.AddControllers();
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
}
