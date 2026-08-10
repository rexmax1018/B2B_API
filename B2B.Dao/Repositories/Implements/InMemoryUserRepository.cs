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

    /// <inheritdoc />
    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = Users.FirstOrDefault(x => string.Equals(x.Account, account, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user?.Clone());
    }

    /// <inheritdoc />
    public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = Users.FirstOrDefault(x => x.UserId == userId);
        return Task.FromResult(user?.Clone());
    }
}

internal static class UserDomainCloneExtensions
{
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
