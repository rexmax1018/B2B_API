using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using B2B.Domain;
using B2B.Service.Impl.Mappings;
using B2B.Service.Interfaces;
using B2B.Service.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供 JWT Access Token 與 Refresh Token 產生服務。
/// </summary>
/// <param name="options">JWT 設定。</param>
public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    /// <summary>
    /// 依服務身分產生 JWT Access Token 與 Refresh Token。
    /// </summary>
    /// <param name="service">服務身分資料。</param>
    /// <returns>簽發的權杖資料。</returns>
    public TokenDomain GenerateToken(ServiceDomain service)
    {
        var jwt = options.Value;
        var now = DateTime.UtcNow;
        var accessExpiresAt = now.AddMinutes(jwt.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(jwt.RefreshTokenDays);

        var accessToken = GenerateJwt(service.ToJwtClaims(), jwt.Audience, accessExpiresAt, now, jwt);
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
        JwtOptions jwt)
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
    private static SymmetricSecurityKey CreateSigningKey(JwtOptions jwt) =>
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

}
