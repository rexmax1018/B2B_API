using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        return await userRepository.GetByAccountAsync(account, cancellationToken);
    }

    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(userId, cancellationToken);
    }
}
