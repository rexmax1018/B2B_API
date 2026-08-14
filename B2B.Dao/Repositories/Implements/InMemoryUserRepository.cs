using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;

namespace B2B.Dao.Repositories.Implements;

/// <summary>
/// 提供開發與測試用的記憶體使用者資料來源。
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly object syncRoot = new();
    private readonly List<UserDomain> users =
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
    private long nextUserId = 2;

    /// <inheritdoc />
    public Task<IReadOnlyList<UserDomain>> GetListAsync(UserFind? find, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            IEnumerable<UserDomain> query = users;

            if (find?.UserId is { } userId)
            {
                query = query.Where(x => x.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(find?.Account))
            {
                var accountKeyword = find.Account.Trim();
                query = query.Where(x => x.Account.Contains(accountKeyword, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(find?.DisplayName))
            {
                var displayNameKeyword = find.DisplayName.Trim();
                query = query.Where(x => x.DisplayName.Contains(displayNameKeyword, StringComparison.OrdinalIgnoreCase));
            }

            if (find?.IsActive is { } isActive)
            {
                query = query.Where(x => x.IsActive == isActive);
            }

            if (find?.CreatedAtFrom is { } createdAtFrom)
            {
                query = query.Where(x => x.CreatedAt >= createdAtFrom);
            }

            if (find?.CreatedAtTo is { } createdAtTo)
            {
                query = query.Where(x => x.CreatedAt <= createdAtTo);
            }

            return Task.FromResult<IReadOnlyList<UserDomain>>(
                query.OrderBy(x => x.UserId).Select(x => x.Clone()).ToArray());
        }
    }

    /// <inheritdoc />
    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (syncRoot)
        {
            var user = users.FirstOrDefault(x => string.Equals(x.Account, account, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user?.Clone());
        }
    }

    /// <inheritdoc />
    public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (syncRoot)
        {
            var user = users.FirstOrDefault(x => x.UserId == userId);
            return Task.FromResult(user?.Clone());
        }
    }

    /// <inheritdoc />
    public Task<UserDomain> InsertAsync(UserDomain user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var inserted = user.Clone();
            inserted.UserId = nextUserId++;
            if (inserted.CreatedAt == default)
            {
                inserted.CreatedAt = DateTime.UtcNow;
            }

            users.Add(inserted);
            return Task.FromResult(inserted.Clone());
        }
    }

    /// <inheritdoc />
    public Task<UserDomain?> UpdateAsync(UserDomain user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var existing = users.FirstOrDefault(x => x.UserId == user.UserId);
            if (existing is null)
            {
                return Task.FromResult<UserDomain?>(null);
            }

            existing.Account = user.Account;
            existing.DisplayName = user.DisplayName;
            existing.PasswordHash = user.PasswordHash;
            existing.IsActive = user.IsActive;
            return Task.FromResult<UserDomain?>(existing.Clone());
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var user = users.FirstOrDefault(x => x.UserId == userId);
            return Task.FromResult(user is not null && users.Remove(user));
        }
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
