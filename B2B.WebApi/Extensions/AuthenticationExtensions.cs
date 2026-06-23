using System.Text;
using B2B.Service.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace B2B.WebApi.Extensions;

/// <summary>
/// 提供驗證與授權服務註冊方法。
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 加入 B2B API 的 JWT Bearer 驗證設定。
    /// </summary>
    /// <param name="services">服務集合。</param>
    /// <param name="configuration">應用程式設定。</param>
    /// <returns>服務集合。</returns>
    public static IServiceCollection AddB2BAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("必須設定 Jwt 區段。");

        SecurityConfigurationValidator.ValidateJwtSecret(jwtOptions.SecretKey);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
