using B2B.Dao.Contexts;
using B2B.Dao.Mappings;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Repositories.Implements;

/// <summary>
/// 透過 EF Core 查詢使用者資料。
/// </summary>
/// <param name="dbContext">B2B 資料庫內容。</param>
public sealed class UserRepository(B2BDbContext dbContext) : IUserRepository
{
    /// <summary>
    /// 透過資料庫依登入帳號取得使用者資料。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Account == account, cancellationToken);

        return entity?.ToModel();
    }

    /// <summary>
    /// 透過資料庫依使用者識別碼取得使用者資料。
    /// </summary>
    /// <param name="userId">使用者識別碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return entity?.ToModel();
    }
}
