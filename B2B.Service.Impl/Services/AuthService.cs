using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供服務憑證登入、權杖換發與登出服務。
/// </summary>
/// <param name="userRepository">使用者資料來源。</param>
/// <param name="tokenService">權杖簽發服務。</param>
/// <param name="refreshTokenStore">Refresh Token 儲存服務。</param>
public sealed class AuthService(
    IEntryCredentialValidator entryCredentialValidator,
    ITokenService tokenService,
    IRefreshTokenStore refreshTokenStore) : IAuthService
{
    private const string InvalidRefreshTokenCode = "INVALID_REFRESH_TOKEN";
    private const string RefreshTokenExpiredCode = "REFRESH_TOKEN_EXPIRED";
    private const string RefreshTokenRevokedCode = "REFRESH_TOKEN_REVOKED";
    private const string RefreshTokenInvalidMessage = "登入狀態已失效，請重新登入";
    private const string EntryCredentialServiceId = "entry-credential";
    private const string EntryCredentialServiceName = "Entry Credential";

    /// <summary>
    /// 驗證 AES 加密 Entry 憑證，成功時簽發權杖並保存 Refresh Token。
    /// </summary>
    /// <param name="encryptedCredential">其他專案傳入的 AES 加密 Entry 憑證。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入處理結果。</returns>
    public async Task<LoginResultDomain> LoginAsync(
        string encryptedCredential,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!entryCredentialValidator.IsValid(encryptedCredential))
        {
            return LoginResultDomain.Failed("憑證驗證失敗", "INVALID_ENTRY_CREDENTIAL");
        }

        var service = CreateEntryCredentialService();
        var token = tokenService.GenerateToken(service);
        await SaveRefreshTokenAsync(service, token, cancellationToken);

        return LoginResultDomain.Succeeded(token);
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

        var service = CreateEntryCredentialService();

        var newToken = tokenService.GenerateToken(service);
        await SaveRefreshTokenAsync(service, newToken, cancellationToken);

        return LoginResultDomain.Succeeded(newToken);
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
    /// <param name="service">服務身分資料。</param>
    /// <param name="token">權杖資料。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>儲存作業。</returns>
    private Task SaveRefreshTokenAsync(
        ServiceDomain service,
        TokenDomain token,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiresIn = token.RefreshTokenExpiresAt - now;

        var model = new RefreshTokenModel
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
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
    /// 建立 Entry 憑證代表的服務身分。
    /// </summary>
    /// <returns>服務身分資料。</returns>
    private static ServiceDomain CreateEntryCredentialService()
    {
        return new ServiceDomain
        {
            ServiceId = EntryCredentialServiceId,
            ServiceName = EntryCredentialServiceName
        };
    }
}
