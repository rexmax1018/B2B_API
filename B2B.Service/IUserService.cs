using B2B.Domain;

namespace B2B.Service.Interfaces;

public interface IUserService
{
    Task<UserDomain?> GetByAccountAsync(
        string account,
        CancellationToken cancellationToken);

    Task<UserDomain?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken);
}
