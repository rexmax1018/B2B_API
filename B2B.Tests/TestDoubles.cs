using System.Security.Cryptography;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Interfaces;

namespace B2B.Tests;

/// <summary>
/// 測試用的記憶體使用者資料來源。
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<long, UserDomain> usersById = [];
    private readonly Dictionary<string, UserDomain> usersByAccount = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 加入測試使用者。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    public void Add(UserDomain user)
    {
        usersById[user.UserId] = Clone(user);
        usersByAccount[user.Account] = Clone(user);
    }

    /// <summary>
    /// 從測試資料來源依登入帳號取得使用者資料。
    /// </summary>
    /// <param name="account">登入帳號。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(usersByAccount.TryGetValue(account, out var user)
            ? Clone(user)
            : null);
    }

    /// <summary>
    /// 從測試資料來源依使用者識別碼取得使用者資料。
    /// </summary>
    /// <param name="userId">使用者識別碼。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>使用者資料；找不到時為 <see langword="null"/>。</returns>
    public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(usersById.TryGetValue(userId, out var user)
            ? Clone(user)
            : null);
    }

    /// <summary>
    /// 複製使用者資料，避免測試共用可變狀態。
    /// </summary>
    /// <param name="user">來源使用者。</param>
    /// <returns>複製後的使用者。</returns>
    private static UserDomain Clone(UserDomain user) => new()
    {
        UserId = user.UserId,
        Account = user.Account,
        DisplayName = user.DisplayName,
        PasswordHash = user.PasswordHash,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}

/// <summary>
/// 依序回傳預先排入權杖的測試服務。
/// </summary>
internal sealed class QueueTokenService : ITokenService
{
    private readonly Queue<TokenDomain> tokens = [];

    /// <summary>
    /// 取得產生權杖方法被呼叫的次數。
    /// </summary>
    public int GenerateTokenCallCount { get; private set; }

    /// <summary>
    /// 排入下一次要回傳的權杖。
    /// </summary>
    /// <param name="token">權杖資料。</param>
    public void Enqueue(TokenDomain token) => tokens.Enqueue(token);

    /// <summary>
    /// 取得下一個預先排入的測試權杖。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    /// <returns>測試權杖資料。</returns>
    public TokenDomain GenerateToken(UserDomain user)
    {
        GenerateTokenCallCount++;

        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("尚未排入測試權杖。");
        }

        return tokens.Dequeue();
    }
}

/// <summary>
/// 可觀察儲存與移除結果的 Refresh Token 測試儲存區。
/// </summary>
internal sealed class SpyRefreshTokenStore : IRefreshTokenStore
{
    private readonly Dictionary<string, RefreshTokenModel> tokens = [];

    /// <summary>
    /// 取得已儲存的 Refresh Token。
    /// </summary>
    public IReadOnlyDictionary<string, RefreshTokenModel> SavedTokens => tokens;

    /// <summary>
    /// 取得已移除的 Refresh Token。
    /// </summary>
    public List<string> RemovedTokens { get; } = [];

    /// <summary>
    /// 預先放入 Refresh Token。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="model">Refresh Token 資料。</param>
    public void Seed(string refreshToken, RefreshTokenModel model)
    {
        tokens[refreshToken] = Clone(model);
    }

    /// <summary>
    /// 將 Refresh Token 寫入測試儲存區。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="model">Refresh Token 資料。</param>
    /// <param name="expiresIn">權杖有效時間。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>儲存作業。</returns>
    public Task SaveAsync(
        string refreshToken,
        RefreshTokenModel model,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        tokens[refreshToken] = Clone(model);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 從測試儲存區取得 Refresh Token 資料。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>Refresh Token 資料；找不到時為 <see langword="null"/>。</returns>
    public Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(tokens.TryGetValue(refreshToken, out var model)
            ? Clone(model)
            : null);
    }

    /// <summary>
    /// 從測試儲存區移除 Refresh Token 並記錄移除清單。
    /// </summary>
    /// <param name="refreshToken">Refresh Token。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>移除作業。</returns>
    public Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RemovedTokens.Add(refreshToken);
        tokens.Remove(refreshToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 複製 Refresh Token 資料，避免測試共用可變狀態。
    /// </summary>
    /// <param name="model">來源 Refresh Token 資料。</param>
    /// <returns>複製後的 Refresh Token 資料。</returns>
    private static RefreshTokenModel Clone(RefreshTokenModel model) => new()
    {
        UserId = model.UserId,
        Account = model.Account,
        CreatedAt = model.CreatedAt,
        ExpiresAt = model.ExpiresAt,
        IsRevoked = model.IsRevoked
    };
}

/// <summary>
/// 建立測試用密碼雜湊。
/// </summary>
internal static class PasswordHashBuilder
{
    /// <summary>
    /// 建立 PBKDF2-SHA256 密碼雜湊字串。
    /// </summary>
    /// <param name="password">原始密碼。</param>
    /// <param name="iterations">雜湊迭代次數。</param>
    /// <returns>密碼雜湊字串。</returns>
    public static string CreatePbkdf2Sha256(string password, int iterations = 100_000)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2-SHA256:{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
