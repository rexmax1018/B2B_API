using B2B.WebApi.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

namespace B2B.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
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

        return services;
    }

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
