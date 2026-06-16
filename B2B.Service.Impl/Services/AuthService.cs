using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Service.Interfaces;
using Microsoft.Extensions.Configuration;

namespace B2B.Service.Impl.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IConfiguration configuration) : IAuthService
{
    public async Task<LoginResultDomain> LoginAsync(
        string account,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByAccountAsync(account, cancellationToken);

        if (user is not null && user.IsActive && VerifyPassword(password, user.PasswordHash))
        {
            var token = tokenService.GenerateToken(user);

            return LoginResultDomain.Succeeded(user, token);
        }

        if (!IsDirectJwtIssueEnabled())
        {
            return LoginResultDomain.Failed("帳號或密碼錯誤");
        }

        var directUser = CreateDirectJwtUser(account, user);
        var directToken = tokenService.GenerateToken(directUser);

        return LoginResultDomain.Succeeded(directUser, directToken);
    }

    public async Task<LoginResultDomain> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var userId = tokenService.GetRefreshTokenUserId(refreshToken);

        if (userId is null)
        {
            return LoginResultDomain.Failed("Refresh Token 無效或已逾期");
        }

        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return LoginResultDomain.Failed("使用者不存在或已停用");
        }

        var newToken = tokenService.GenerateToken(user);

        return LoginResultDomain.Succeeded(user, newToken);
    }

    private static bool VerifyPassword(string password, string storedPasswordHash)
    {
        if (storedPasswordHash.StartsWith("PLAIN:", StringComparison.Ordinal))
        {
            return string.Equals(
                password,
                storedPasswordHash["PLAIN:".Length..],
                StringComparison.Ordinal);
        }

        // 預留正式環境替換點：可改為 BCrypt/Argon2/PBKDF2 等雜湊驗證。
        return string.Equals(password, storedPasswordHash, StringComparison.Ordinal);
    }

    private bool IsDirectJwtIssueEnabled() =>
        bool.TryParse(configuration["Authentication:EnableDirectJwtIssue"], out var enabled) && enabled;

    private UserDomain CreateDirectJwtUser(string account, UserDomain? existingUser)
    {
        if (existingUser is not null)
        {
            return new UserDomain
            {
                UserId = existingUser.UserId,
                Account = existingUser.Account,
                DisplayName = existingUser.DisplayName,
                PasswordHash = existingUser.PasswordHash,
                IsActive = true,
                CreatedAt = existingUser.CreatedAt
            };
        }

        return new UserDomain
        {
            UserId = ReadLong(configuration["Authentication:DirectJwtUserId"], 1),
            Account = account,
            DisplayName = configuration["Authentication:DirectJwtDisplayName"] ?? account,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static long ReadLong(string? value, long defaultValue)
    {
        return long.TryParse(value, out var parsedValue) ? parsedValue : defaultValue;
    }
}
