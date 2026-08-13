using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Impl.Services;

namespace B2B.Tests;

/// <summary>
/// 驗證 AuthService 的登入與 Refresh Token 流程。
/// </summary>
public sealed class AuthServiceTests
{
    /// <summary>
    /// 驗證舊版登入驗證尚未接回時不會簽發權杖。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WhenCredentialValidationIsNotMigrated_ReturnsNotConfigured()
    {
        var tokenService = new QueueTokenService();
        var refreshTokenStore = new SpyRefreshTokenStore();
        var service = new AuthService(tokenService, refreshTokenStore);

        var result = await service.LoginAsync("credential", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AUTHENTICATION_NOT_CONFIGURED", result.ErrorCode);
        Assert.Equal(0, tokenService.GenerateTokenCallCount);
        Assert.Empty(refreshTokenStore.SavedTokens);
    }

    /// <summary>
    /// 驗證不存在的 Refresh Token 會回傳無效權杖錯誤。
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsMissing_ReturnsInvalidRefreshToken()
    {
        var service = new AuthService(new QueueTokenService(), new SpyRefreshTokenStore());

        var result = await service.RefreshTokenAsync("missing-token", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
    }

    /// <summary>
    /// 驗證過期的 Refresh Token 會被移除並回傳過期錯誤。
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpired_RemovesTokenAndReturnsExpiredError()
    {
        var refreshTokenStore = new SpyRefreshTokenStore();
        refreshTokenStore.Seed("expired-token", new RefreshTokenModel
        {
            ServiceId = "service-1",
            ServiceName = "測試服務",
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsRevoked = false
        });

        var service = new AuthService(new QueueTokenService(), refreshTokenStore);

        var result = await service.RefreshTokenAsync("expired-token", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("REFRESH_TOKEN_EXPIRED", result.ErrorCode);
        Assert.Contains("expired-token", refreshTokenStore.RemovedTokens);
        Assert.False(refreshTokenStore.SavedTokens.ContainsKey("expired-token"));
    }

    /// <summary>
    /// 驗證已撤銷的 Refresh Token 會被移除並回傳撤銷錯誤。
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsRevoked_RemovesTokenAndReturnsRevokedError()
    {
        var refreshTokenStore = new SpyRefreshTokenStore();
        refreshTokenStore.Seed("revoked-token", new RefreshTokenModel
        {
            ServiceId = "service-1",
            ServiceName = "測試服務",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        });

        var service = new AuthService(new QueueTokenService(), refreshTokenStore);

        var result = await service.RefreshTokenAsync("revoked-token", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("REFRESH_TOKEN_REVOKED", result.ErrorCode);
        Assert.Contains("revoked-token", refreshTokenStore.RemovedTokens);
    }

    /// <summary>
    /// 驗證有效 Refresh Token 可換發新權杖並移除舊權杖。
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesRefreshToken()
    {
        var refreshTokenStore = new SpyRefreshTokenStore();
        refreshTokenStore.Seed("old-refresh-token", new RefreshTokenModel
        {
            ServiceId = "service-1",
            ServiceName = "測試服務",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        });

        var newToken = CreateToken("new-refresh-token");
        var tokenService = new QueueTokenService();
        tokenService.Enqueue(newToken);
        var service = new AuthService(tokenService, refreshTokenStore);

        var result = await service.RefreshTokenAsync("old-refresh-token", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Same(newToken, result.Token);
        Assert.Contains("old-refresh-token", refreshTokenStore.RemovedTokens);
        Assert.False(refreshTokenStore.SavedTokens.ContainsKey("old-refresh-token"));
        Assert.True(refreshTokenStore.SavedTokens.ContainsKey(newToken.RefreshToken));
    }

    private static TokenDomain CreateToken(string refreshToken) => new()
    {
        AccessToken = $"access-{refreshToken}",
        RefreshToken = refreshToken,
        TokenType = "Bearer",
        ExpiresIn = 3600,
        AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
    };
}
