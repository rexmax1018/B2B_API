using System.Security.Cryptography;
using System.Text;
using B2B.Domain.Models;
using B2B.Service.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace B2B.Service.Impl.Stores;

/// <summary>
/// 使用記憶體快取儲存 Refresh Token。
/// </summary>
/// <param name="memoryCache">記憶體快取。</param>
public class MemoryRefreshTokenStore(IMemoryCache memoryCache) : IRefreshTokenStore
{
    /// <summary>
    /// 將 Refresh Token 資料寫入記憶體快取。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="model">Refresh Token 關聯資料。</param>
    /// <param name="expiresIn">快取有效時間。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>儲存作業。</returns>
    public Task SaveAsync(
        string refreshToken,
        RefreshTokenModel model,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        memoryCache.Set(
            key,
            model,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiresIn
            });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 從記憶體快取取得 Refresh Token 關聯資料。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>Refresh Token 關聯資料；找不到時為 <see langword="null"/>。</returns>
    public Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        memoryCache.TryGetValue(key, out RefreshTokenModel? model);

        return Task.FromResult(model);
    }

    /// <summary>
    /// 從記憶體快取移除 Refresh Token。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>移除作業。</returns>
    public Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        memoryCache.Remove(key);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 依 Refresh Token 建立快取鍵。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <returns>快取鍵。</returns>
    private static string BuildKey(string refreshToken)
    {
        var hash = Sha256(refreshToken);
        return $"refresh_token:{hash}";
    }

    /// <summary>
    /// 計算字串的 SHA-256 雜湊。
    /// </summary>
    /// <param name="value">來源字串。</param>
    /// <returns>十六進位雜湊字串。</returns>
    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
