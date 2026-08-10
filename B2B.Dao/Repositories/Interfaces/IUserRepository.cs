using B2B.Domain;

namespace B2B.Dao.Repositories.Interfaces;

/// <summary>
/// 定義使用者資料存取介面。
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 依登入帳號取得使用者資料。
    /// </summary>
    Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken);

    /// <summary>
    /// 依使用者識別碼取得使用者資料。
    /// </summary>
    Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken);
}
