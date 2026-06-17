using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供使用者查詢服務。
/// </summary>
/// <param name="userRepository">使用者資料來源。</param>
public sealed class UserService(IUserRepository userRepository) : IUserService
{
    /// <summary>
    /// 依登入帳號查詢使用者資料。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        return await userRepository.GetByAccountAsync(account, cancellationToken);
    }

    /// <summary>
    /// 依使用者識別碼查詢使用者資料。
    /// </summary>
    /// <param name="userId">使用者識別碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(userId, cancellationToken);
    }
}
