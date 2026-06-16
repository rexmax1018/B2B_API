using B2B.Domain;

namespace B2B.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResultDomain> LoginAsync(
        string account,
        string password,
        CancellationToken cancellationToken);

    Task<LoginResultDomain> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}
