using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;

namespace B2B.Dao.Repositories.Implements;

public sealed class InMemoryUserRepository : IUserRepository
{
    private static readonly IReadOnlyList<UserDomain> Users =
    [
        new()
        {
            UserId = 1,
            Account = "admin",
            DisplayName = "Administrator",
            // 開發測試用：正式環境請改由安全雜湊演算法與鹽值保存密碼。
            PasswordHash = "PLAIN:123456",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    ];

    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = Users.FirstOrDefault(x =>
            string.Equals(x.Account, account, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user?.Clone());
    }

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
