using B2B.Domain;

namespace B2B.Service.Interfaces;

/// <summary>
/// 定義由 Web API 使用的使用者查詢服務。
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 依登入帳號查詢使用者。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken);

    /// <summary>
    /// 依使用者識別碼查詢使用者。
    /// </summary>
    /// <param name="userId">使用者識別碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken);
}
