using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供服務登入、權杖換發與登出服務。
/// </summary>
/// <param name="tokenService">權杖簽發服務。</param>
/// <param name="refreshTokenStore">Refresh Token 儲存服務。</param>
public sealed class AuthService(
    ITokenService tokenService,
    IRefreshTokenStore refreshTokenStore) : IAuthService
{
    private const string InvalidRefreshTokenCode = "INVALID_REFRESH_TOKEN";
    private const string RefreshTokenExpiredCode = "REFRESH_TOKEN_EXPIRED";
    private const string RefreshTokenRevokedCode = "REFRESH_TOKEN_REVOKED";
    private const string RefreshTokenInvalidMessage = "登入狀態已失效，請重新登入";
    private const string AuthenticationNotConfiguredCode = "AUTHENTICATION_NOT_CONFIGURED";

    /// <summary>
    /// 驗證應用程式憑證，成功時簽發權杖並保存 Refresh Token。
    /// </summary>
    /// <param name="credential">呼叫端提供的憑證。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入處理結果。</returns>
    public async Task<LoginResultDomain> LoginAsync(
        string credential,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: 在此接回舊版驗證。驗證成功後建立已驗證的 ServiceDomain，並呼叫 IssueTokenAsync。
        // return await IssueTokenAsync(authenticatedService, cancellationToken);
        return LoginResultDomain.Failed("登入驗證尚未設定", AuthenticationNotConfiguredCode);
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

        if (string.IsNullOrWhiteSpace(storedToken.ServiceId) ||
            string.IsNullOrWhiteSpace(storedToken.ServiceName))
        {
            return LoginResultDomain.Failed(RefreshTokenInvalidMessage, InvalidRefreshTokenCode);
        }

        var service = new ServiceDomain
        {
            ServiceId = storedToken.ServiceId,
            ServiceName = storedToken.ServiceName
        };

        return await IssueTokenAsync(service, cancellationToken);
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
    /// 依已驗證的服務身分簽發 Token 並保存 Refresh Token。
    /// </summary>
    /// <param name="service">服務身分資料。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入處理結果。</returns>
    private async Task<LoginResultDomain> IssueTokenAsync(
        ServiceDomain service,
        CancellationToken cancellationToken)
    {
        var token = tokenService.GenerateToken(service);
        await SaveRefreshTokenAsync(service, token, cancellationToken);

        return LoginResultDomain.Succeeded(token);
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

}
