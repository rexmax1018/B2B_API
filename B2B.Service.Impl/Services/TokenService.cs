using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using B2B.Domain;
using B2B.Service.Impl.Mappings;
using B2B.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供 JWT Access Token 與 Refresh Token 產生服務。
/// </summary>
/// <param name="configuration">應用程式設定。</param>
public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    /// <summary>
    /// 依使用者資料產生 JWT Access Token 與 Refresh Token。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    /// <returns>簽發的權杖資料。</returns>
    public TokenDomain GenerateToken(UserDomain user)
    {
        var jwt = ReadJwtSettings();
        var now = DateTime.UtcNow;
        var accessExpiresAt = now.AddMinutes(jwt.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(jwt.RefreshTokenDays);

        var accessToken = GenerateJwt(user.ToJwtClaims(), jwt.Audience, accessExpiresAt, now, jwt);
        var refreshToken = GenerateRefreshToken();

        return new TokenDomain
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = (int)TimeSpan.FromMinutes(jwt.AccessTokenMinutes).TotalSeconds,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    /// <summary>
    /// 建立已簽章的 JWT 字串。
    /// </summary>
    /// <param name="claims">JWT Claims。</param>
    /// <param name="audience">JWT 接收者。</param>
    /// <param name="expiresAt">到期時間。</param>
    /// <param name="now">簽發時間。</param>
    /// <param name="jwt">JWT 設定。</param>
    /// <returns>JWT 字串。</returns>
    private static string GenerateJwt(
        IEnumerable<Claim> claims,
        string audience,
        DateTime expiresAt,
        DateTime now,
        JwtSettings jwt)
    {
        var credentials = new SigningCredentials(CreateSigningKey(jwt), SecurityAlgorithms.HmacSha256);
        var securityToken = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    /// <summary>
    /// 由 JWT 密鑰建立對稱簽章金鑰。
    /// </summary>
    /// <param name="jwt">JWT 設定。</param>
    /// <returns>對稱簽章金鑰。</returns>
    private static SymmetricSecurityKey CreateSigningKey(JwtSettings jwt) =>
        new(Encoding.UTF8.GetBytes(jwt.SecretKey));

    /// <summary>
    /// 產生 URL 安全的 Refresh Token。
    /// </summary>
    /// <returns>Refresh Token。</returns>
    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// 從設定來源讀取 JWT 設定。
    /// </summary>
    /// <returns>JWT 設定。</returns>
    private JwtSettings ReadJwtSettings()
    {
        var section = configuration.GetSection("Jwt");

        return new JwtSettings
        {
            Issuer = section["Issuer"] ?? "B2B_API",
            Audience = section["Audience"] ?? "B2B_API_CLIENT",
            SecretKey = section["SecretKey"] ?? throw new InvalidOperationException("必須設定 Jwt:SecretKey。"),
            AccessTokenMinutes = ReadInt(section["AccessTokenMinutes"], 60),
            RefreshTokenDays = ReadInt(section["RefreshTokenDays"], 7)
        };
    }

    /// <summary>
    /// 讀取整數設定值，無法解析時使用預設值。
    /// </summary>
    /// <param name="value">設定值。</param>
    /// <param name="defaultValue">預設值。</param>
    /// <returns>解析後的整數。</returns>
    private static int ReadInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) ? parsedValue : defaultValue;
    }

    /// <summary>
    /// 表示 JWT 簽發所需設定。
    /// </summary>
    private sealed class JwtSettings
    {
        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public string SecretKey { get; init; } = string.Empty;

        public int AccessTokenMinutes { get; init; }

        public int RefreshTokenDays { get; init; }
    }
}
