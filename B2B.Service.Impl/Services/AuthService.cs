using System.Security.Cryptography;
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
    private const int MinPbkdf2Iterations = 100_000;
    private const int ExpectedPbkdf2Parts = 4;

    /// <summary>
    /// 驗證登入帳號與密碼，成功時簽發權杖並保存 Refresh Token。
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
        var user = await userRepository.GetByAccountAsync(account, cancellationToken);

        if (user is not null && user.IsActive && VerifyPassword(password, user.PasswordHash))
        {
            var token = tokenService.GenerateToken(user);
            await SaveRefreshTokenAsync(user, token, cancellationToken);

            return LoginResultDomain.Succeeded(user, token);
        }

        return LoginResultDomain.Failed("帳號或密碼錯誤");
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
        var storedToken = await refreshTokenStore.GetAsync(refreshToken, cancellationToken);

        if (storedToken is null)
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, InvalidRefreshTokenCode);
        }

        if (DateTime.UtcNow >= storedToken.ExpiresAt)
        {
            await refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, RefreshTokenExpiredCode);
        }

        if (storedToken.IsRevoked)
        {
            await refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, RefreshTokenRevokedCode);
        }

        var user = await userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, InvalidRefreshTokenCode);
        }

        await refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

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
    /// 驗證輸入密碼是否符合儲存的 PBKDF2-SHA256 雜湊。
    /// </summary>
    /// <param name="password">輸入密碼。</param>
    /// <param name="storedPasswordHash">儲存的密碼雜湊。</param>
    /// <returns>密碼相符時為 <see langword="true"/>。</returns>
    private static bool VerifyPassword(string password, string storedPasswordHash)
    {
        var parts = storedPasswordHash.Split(':', ExpectedPbkdf2Parts);

        if (parts.Length != ExpectedPbkdf2Parts ||
            !string.Equals(parts[0], "PBKDF2-SHA256", StringComparison.Ordinal) ||
            !int.TryParse(parts[1], out var iterations) ||
            iterations < MinPbkdf2Iterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
