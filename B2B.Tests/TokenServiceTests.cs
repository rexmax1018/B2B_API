using System.IdentityModel.Tokens.Jwt;
using B2B.Domain;
using B2B.Service.Impl.Services;
using B2B.Service.Options;
using Microsoft.Extensions.Options;

namespace B2B.Tests;

/// <summary>
/// 驗證 TokenService 的 JWT 與 Refresh Token 產生行為。
/// </summary>
public sealed class TokenServiceTests
{
    /// <summary>
    /// 驗證 JWT 設定會產生簽章 Access Token 與 Refresh Token。
    /// </summary>
    [Fact]
    public void GenerateToken_WithConfiguredJwtOptions_CreatesSignedAccessTokenAndRefreshToken()
    {
        var service = new TokenService(Options.Create(new JwtOptions
        {
            Issuer = "B2B_API_TEST",
            Audience = "B2B_API_TEST_CLIENT",
            SecretKey = "test-secret-key-with-at-least-32-characters",
            AccessTokenMinutes = 30,
            RefreshTokenDays = 14
        }));
        var serviceIdentity = new ServiceDomain
        {
            ServiceId = "test-service",
            ServiceName = "測試服務"
        };

        var token = service.GenerateToken(serviceIdentity);

        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(1800, token.ExpiresIn);
        Assert.NotEmpty(token.AccessToken);
        Assert.NotEmpty(token.RefreshToken);
        Assert.DoesNotContain("+", token.RefreshToken);
        Assert.DoesNotContain("/", token.RefreshToken);
        Assert.DoesNotContain("=", token.RefreshToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.Equal("B2B_API_TEST", jwt.Issuer);
        Assert.Contains("B2B_API_TEST_CLIENT", jwt.Audiences);
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == serviceIdentity.ServiceId);
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == "service_name" && claim.Value == serviceIdentity.ServiceName);
        Assert.InRange(token.AccessTokenExpiresAt, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
        Assert.InRange(token.RefreshTokenExpiresAt, DateTime.UtcNow.AddDays(13), DateTime.UtcNow.AddDays(15));
    }
}
