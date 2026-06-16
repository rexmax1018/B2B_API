using B2B.Domain;

namespace B2B.Dao.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken);

    Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken);
}
