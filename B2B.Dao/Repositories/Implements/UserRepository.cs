using B2B.Dao.Contexts;
using B2B.Dao.Mappings;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Repositories.Implements;

/// <summary>
/// 透過 EF Core 查詢使用者資料。
/// </summary>
public sealed class UserRepository(B2BDbContext dbContext) : IUserRepository
{
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
}
