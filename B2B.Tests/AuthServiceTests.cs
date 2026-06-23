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
    /// 驗證有效帳密會簽發權杖並儲存 Refresh Token。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndStoresRefreshToken()
    {
        var repository = new FakeUserRepository();
        var user = CreateActiveUser(password: "Valid-password-1");
        repository.Add(user);

        var issuedToken = CreateToken("refresh-token-1");
        var tokenService = new QueueTokenService();
        tokenService.Enqueue(issuedToken);
        var refreshTokenStore = new SpyRefreshTokenStore();
        var service = new AuthService(repository, tokenService, refreshTokenStore);

        var result = await service.LoginAsync(user.Account, "Valid-password-1", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Same(issuedToken, result.Token);
        Assert.Equal(1, tokenService.GenerateTokenCallCount);
        Assert.True(refreshTokenStore.SavedTokens.ContainsKey(issuedToken.RefreshToken));
        Assert.Equal(user.UserId, refreshTokenStore.SavedTokens[issuedToken.RefreshToken].UserId);
    }

    /// <summary>
    /// 驗證密碼錯誤時仍會暫時放行並簽發權杖。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsTokenTemporarily()
    {
        var repository = new FakeUserRepository();
        var user = CreateActiveUser(password: "Valid-password-1");
        repository.Add(user);

        var issuedToken = CreateToken("refresh-token-2");
        var tokenService = new QueueTokenService();
        tokenService.Enqueue(issuedToken);
        var refreshTokenStore = new SpyRefreshTokenStore();
        var service = new AuthService(repository, tokenService, refreshTokenStore);

        var result = await service.LoginAsync(user.Account, "wrong-password", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Same(issuedToken, result.Token);
        Assert.Equal(1, tokenService.GenerateTokenCallCount);
        Assert.True(refreshTokenStore.SavedTokens.ContainsKey(issuedToken.RefreshToken));
    }

    /// <summary>
    /// 驗證不存在的 Refresh Token 會回傳無效權杖錯誤。
    /// </summary>
    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsMissing_ReturnsInvalidRefreshToken()
    {
        var service = new AuthService(
            new FakeUserRepository(),
            new QueueTokenService(),
            new SpyRefreshTokenStore());

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
            UserId = 1,
            Account = "user1",
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsRevoked = false
        });

        var service = new AuthService(
            new FakeUserRepository(),
            new QueueTokenService(),
            refreshTokenStore);

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
            UserId = 1,
            Account = "user1",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        });

        var service = new AuthService(
            new FakeUserRepository(),
            new QueueTokenService(),
            refreshTokenStore);

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
        var repository = new FakeUserRepository();
        var user = CreateActiveUser(password: "Valid-password-1");
        repository.Add(user);

        var refreshTokenStore = new SpyRefreshTokenStore();
        refreshTokenStore.Seed("old-refresh-token", new RefreshTokenModel
        {
            UserId = user.UserId,
            Account = user.Account,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        });

        var newToken = CreateToken("new-refresh-token");
        var tokenService = new QueueTokenService();
        tokenService.Enqueue(newToken);
        var service = new AuthService(repository, tokenService, refreshTokenStore);

        var result = await service.RefreshTokenAsync("old-refresh-token", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Same(newToken, result.Token);
        Assert.Contains("old-refresh-token", refreshTokenStore.RemovedTokens);
        Assert.False(refreshTokenStore.SavedTokens.ContainsKey("old-refresh-token"));
        Assert.True(refreshTokenStore.SavedTokens.ContainsKey(newToken.RefreshToken));
    }

    /// <summary>
    /// 建立啟用狀態的測試使用者。
    /// </summary>
    /// <param name="password">原始密碼。</param>
    /// <returns>測試使用者。</returns>
    private static UserDomain CreateActiveUser(string password) => new()
    {
        UserId = 1,
        Account = "user1",
        DisplayName = "測試使用者",
        PasswordHash = PasswordHashBuilder.CreatePbkdf2Sha256(password),
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// 建立測試用權杖資料。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <returns>權杖資料。</returns>
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
