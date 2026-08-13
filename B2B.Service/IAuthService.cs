using B2B.Domain;

namespace B2B.Service.Interfaces;

/// <summary>
/// 定義驗證與權杖生命週期服務。
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 驗證應用程式憑證並簽發權杖。
    /// </summary>
    /// <param name="credential">呼叫端提供的憑證。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入處理結果。</returns>
    Task<LoginResultDomain> LoginAsync(
        string credential,
        CancellationToken cancellationToken);

    /// <summary>
    /// 使用 Refresh Token 換發新的權杖。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>換發權杖處理結果。</returns>
    Task<LoginResultDomain> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// 登出並撤銷指定的 Refresh Token。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}
