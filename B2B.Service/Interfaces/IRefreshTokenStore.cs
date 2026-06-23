using B2B.Domain.Models;

namespace B2B.Service.Interfaces;

/// <summary>
/// 定義 Refresh Token 儲存與查詢介面。
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// 儲存 Refresh Token。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="model">Refresh Token 關聯資料。</param>
    /// <param name="expiresIn">快取有效時間。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    Task SaveAsync(
        string refreshToken,
        RefreshTokenModel model,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得 Refresh Token 關聯資料。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>Refresh Token 關聯資料；找不到時為 <see langword="null"/>。</returns>
    Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 以原子方式取得並移除 Refresh Token，避免同一權杖被重複換發。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>Refresh Token 關聯資料；找不到時為 <see langword="null"/>。</returns>
    Task<RefreshTokenModel?> ConsumeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除 Refresh Token。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
