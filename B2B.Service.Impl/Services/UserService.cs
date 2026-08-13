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
    public async Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-DAO]: 保留此 IUserRepository 呼叫，將 .NET Framework 4.8 的帳號查詢規則接在回傳結果之後。
        var user = await userRepository.GetByAccountAsync(account, cancellationToken);

        // TODO[MIGRATE-SERVICE]: 搬入舊版帳號查詢的啟用狀態、權限或其他商業規則；不要在此層回傳 WebApi DTO。
        return user;
    }

    /// <inheritdoc />
    public async Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-DAO]: 保留此 IUserRepository 呼叫，將 .NET Framework 4.8 的識別碼查詢規則接在回傳結果之後。
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        // TODO[MIGRATE-SERVICE]: 搬入舊版識別碼查詢的啟用狀態、權限或其他商業規則；不要在此層回傳 WebApi DTO。
        return user;
    }
}
