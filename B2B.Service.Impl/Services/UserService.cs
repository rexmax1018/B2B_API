using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 提供 Web API 使用者查詢服務。
/// </summary>
/// <param name="userRepository">使用者資料來源。</param>
public sealed class UserService(IUserRepository userRepository) : IUserService
{
    /// <inheritdoc />
    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken) =>
        userRepository.GetByAccountAsync(account, cancellationToken);

    /// <inheritdoc />
    public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken) =>
        userRepository.GetByIdAsync(userId, cancellationToken);
}
