using B2B.Dao.Contexts;
using B2B.Dao.Entities;
using B2B.Dao.Mappings;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Repositories.Implements;

/// <summary>
/// 透過 EF Core 存取使用者資料。
/// </summary>
public sealed class UserRepository(B2BDbContext dbContext) : IUserRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDomain>> GetListAsync(UserFind? find, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-DAO]: 將舊版清單的篩選、排序與分頁規則接到此範例。
        var query = dbContext.Users.AsNoTracking();

        if (find?.UserId is { } userId)
        {
            query = query.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(find?.Account))
        {
            var accountKeyword = find.Account.Trim().ToUpperInvariant();
            query = query.Where(x => x.Account.ToUpper().Contains(accountKeyword));
        }

        if (!string.IsNullOrWhiteSpace(find?.DisplayName))
        {
            var displayNameKeyword = find.DisplayName.Trim().ToUpperInvariant();
            query = query.Where(x => x.DisplayName.ToUpper().Contains(displayNameKeyword));
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

        var entities = await query
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        return entities.Select(x => x.ToModel()).ToArray();
    }

    /// <inheritdoc />
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        var normalizedAccount = account.ToUpperInvariant();
        var entity = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Account.ToUpper() == normalizedAccount, cancellationToken);
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public async Task<UserDomain> InsertAsync(UserDomain user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // TODO[MIGRATE-DAO]: 將舊版新增前的欄位預設值、稽核欄位與驗證規則接到此處。
        var entity = new UserEntity
        {
            Account = user.Account,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt
        };

        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    /// <inheritdoc />
    public async Task<UserDomain?> UpdateAsync(UserDomain user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // TODO[MIGRATE-DAO]: 將舊版更新欄位白名單、並發控制與 UpdatedAt 規則接到此處。
        var entity = await dbContext.Users
            .FirstOrDefaultAsync(x => x.UserId == user.UserId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Account = user.Account;
        entity.DisplayName = user.DisplayName;
        entity.PasswordHash = user.PasswordHash;
        entity.IsActive = user.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long userId, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-DAO]: 將舊版刪除前檢查、軟刪除或關聯資料處理規則接到此處。
        var entity = await dbContext.Users
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.Users.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
