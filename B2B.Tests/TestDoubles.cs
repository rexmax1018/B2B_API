using B2B.Domain;
using B2B.Domain.Models;
using B2B.Service.Interfaces;

namespace B2B.Tests;

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
    /// <param name="service">服務身分資料。</param>
    /// <returns>測試權杖資料。</returns>
    public TokenDomain GenerateToken(ServiceDomain service)
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
/// 測試用的固定 Entry 憑證驗證器。
/// </summary>
/// <param name="expectedCredential">可通過驗證的密文。</param>
internal sealed class FixedEntryCredentialValidator(string expectedCredential) : IEntryCredentialValidator
{
    /// <inheritdoc />
    public bool IsDevelopmentFixture => false;

    /// <inheritdoc />
    public bool IsValid(string? encryptedCredential) =>
        string.Equals(encryptedCredential, expectedCredential, StringComparison.Ordinal);
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(tokens.TryGetValue(refreshToken, out var model)
            ? Clone(model)
            : null);
    }

    /// <inheritdoc />
    public Task<RefreshTokenModel?> ConsumeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!tokens.TryGetValue(refreshToken, out var model))
        {
            return Task.FromResult<RefreshTokenModel?>(null);
        }

        RemovedTokens.Add(refreshToken);
        tokens.Remove(refreshToken);

        return Task.FromResult<RefreshTokenModel?>(Clone(model));
    }

    /// <inheritdoc />
    public Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RemovedTokens.Add(refreshToken);
        tokens.Remove(refreshToken);
        return Task.CompletedTask;
    }

    private static RefreshTokenModel Clone(RefreshTokenModel model) => new()
    {
        ServiceId = model.ServiceId,
        ServiceName = model.ServiceName,
        CreatedAt = model.CreatedAt,
        ExpiresAt = model.ExpiresAt,
        IsRevoked = model.IsRevoked
    };
}
