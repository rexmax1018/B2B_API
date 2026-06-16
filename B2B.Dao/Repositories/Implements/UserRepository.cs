using B2B.Dao.Contexts;
using B2B.Dao.Mappings;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using Microsoft.EntityFrameworkCore;

namespace B2B.Dao.Repositories.Implements;

public sealed class UserRepository(B2BDbContext dbContext) : IUserRepository
{
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Account == account, cancellationToken);

        return entity?.ToModel();
    }

    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return entity?.ToModel();
    }
}
