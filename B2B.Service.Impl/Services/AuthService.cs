using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供帳號登入、權杖換發與登出服務。
/// </summary>
/// <param name="userRepository">使用者資料來源。</param>
/// <param name="tokenService">權杖簽發服務。</param>
/// <param name="refreshTokenStore">Refresh Token 儲存服務。</param>
public sealed class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IRefreshTokenStore refreshTokenStore) : IAuthService
{
    private const string InvalidRefreshTokenCode = "INVALID_REFRESH_TOKEN";
    private const string RefreshTokenExpiredCode = "REFRESH_TOKEN_EXPIRED";
    private const string RefreshTokenRevokedCode = "REFRESH_TOKEN_REVOKED";
    private const string RefreshTokenInvalidMessage = "登入狀態已失效，請重新登入";
    private const long TemporaryUserId = 0;

    /// <summary>
    /// 暫時略過登入帳號與密碼驗證，成功時簽發權杖並保存 Refresh Token。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="password">登入密碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入處理結果。</returns>
    public async Task<LoginResultDomain> LoginAsync(
        string account,
        string password,
        CancellationToken cancellationToken)
    {
        // TODO: 接回正式使用者驗證後，需恢復帳號狀態與密碼檢查。
        var user = await userRepository.GetByAccountAsync(account, cancellationToken)
            ?? CreateTemporaryUser(account, TemporaryUserId);

        var token = tokenService.GenerateToken(user);
        await SaveRefreshTokenAsync(user, token, cancellationToken);

        return LoginResultDomain.Succeeded(user, token);
    }

    /// <summary>
    /// 驗證既有 Refresh Token，成功時移除舊權杖並簽發新權杖。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>權杖換發處理結果。</returns>
    public async Task<LoginResultDomain> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var storedToken = await refreshTokenStore.ConsumeAsync(refreshToken, cancellationToken);

        if (storedToken is null)
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, InvalidRefreshTokenCode);
        }

        if (DateTime.UtcNow >= storedToken.ExpiresAt)
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, RefreshTokenExpiredCode);
        }

        if (storedToken.IsRevoked)
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, RefreshTokenRevokedCode);
        }

        // TODO: 接回正式使用者驗證後，移除暫時使用者 fallback。
        var user = await userRepository.GetByIdAsync(storedToken.UserId, cancellationToken)
            ?? CreateTemporaryUser(storedToken.Account, storedToken.UserId);

        if (user is null || !user.IsActive)
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, InvalidRefreshTokenCode);
        }

        var newToken = tokenService.GenerateToken(user);
        await SaveRefreshTokenAsync(user, newToken, cancellationToken);

        return LoginResultDomain.Succeeded(user, newToken);
    }

    /// <summary>
    /// 移除指定 Refresh Token 以完成登出。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登出作業。</returns>
    public Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        return refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);
    }

    /// <summary>
    /// 儲存新簽發的 Refresh Token。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    /// <param name="token">權杖資料。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>儲存作業。</returns>
    private Task SaveRefreshTokenAsync(
        UserDomain user,
        TokenDomain token,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiresIn = token.RefreshTokenExpiresAt - now;

        var model = new RefreshTokenModel
        {
            UserId = user.UserId,
            Account = user.Account,
            CreatedAt = now,
            ExpiresAt = token.RefreshTokenExpiresAt,
            IsRevoked = false
        };

        return refreshTokenStore.SaveAsync(
            token.RefreshToken,
            model,
            expiresIn,
            cancellationToken);
    }

    /// <summary>
    /// 建立暫時登入使用者。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="userId">使用者識別碼。</param>
    /// <returns>暫時使用者資料。</returns>
    private static UserDomain CreateTemporaryUser(string account, long userId)
    {
        var normalizedAccount = string.IsNullOrWhiteSpace(account)
            ? "temporary-user"
            : account;

        return new UserDomain
        {
            UserId = userId,
            Account = normalizedAccount,
            DisplayName = normalizedAccount,
            PasswordHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
