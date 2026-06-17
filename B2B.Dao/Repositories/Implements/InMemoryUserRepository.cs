using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;

namespace B2B.Dao.Repositories.Implements;

/// <summary>
/// 提供開發與測試用的記憶體使用者資料來源。
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private static readonly IReadOnlyList<UserDomain> Users =
    [
        new()
        {
            UserId = 1,
            Account = "admin",
            DisplayName = "系統管理員",
            PasswordHash = "PBKDF2-SHA256:100000:AQIDBAUGBwgJCgsMDQ4PEA==:4GqoH/SqHM86aKvYpt8G51CnwfVCN1AY5DjzvwZFMtI=",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    ];

    /// <summary>
    /// 從記憶體資料來源依登入帳號取得使用者資料。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = Users.FirstOrDefault(x =>
            string.Equals(x.Account, account, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user?.Clone());
    }

    /// <summary>
    /// 從記憶體資料來源依使用者識別碼取得使用者資料。
    /// </summary>
    /// <param name="userId">使用者識別碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = Users.FirstOrDefault(x => x.UserId == userId);

        return Task.FromResult(user?.Clone());
    }
}

/// <summary>
/// 提供使用者領域模型複製方法。
/// </summary>
internal static class UserDomainCloneExtensions
{
    /// <summary>
    /// 複製使用者領域模型，避免共用可變狀態。
    /// </summary>
    /// <param name="user">來源使用者。</param>
    /// <returns>複製後的使用者。</returns>
    public static UserDomain Clone(this UserDomain user) => new()
    {
        UserId = user.UserId,
        Account = user.Account,
        DisplayName = user.DisplayName,
        PasswordHash = user.PasswordHash,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
