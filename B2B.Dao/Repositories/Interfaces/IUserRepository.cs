using B2B.Domain;

namespace B2B.Dao.Repositories.Interfaces;

/// <summary>
/// 定義使用者資料存取介面。
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 依可選條件取得使用者清單；條件物件為 <see langword="null"/> 或欄位未設定時回傳完整清單。
    /// </summary>
    Task<IReadOnlyList<UserDomain>> GetListAsync(UserFind? find, CancellationToken cancellationToken);

    /// <summary>
    /// 依登入帳號取得使用者資料。
    /// </summary>
    Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken);

    /// <summary>
    /// 依使用者識別碼取得使用者資料。
    /// </summary>
    Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// 新增使用者。
    /// </summary>
    Task<UserDomain> InsertAsync(UserDomain user, CancellationToken cancellationToken);

    /// <summary>
    /// 更新使用者；找不到資料時回傳 <see langword="null"/>。
    /// </summary>
    Task<UserDomain?> UpdateAsync(UserDomain user, CancellationToken cancellationToken);

    /// <summary>
    /// 刪除使用者；找不到資料時回傳 <see langword="false"/>。
    /// </summary>
    Task<bool> DeleteAsync(long userId, CancellationToken cancellationToken);
}
