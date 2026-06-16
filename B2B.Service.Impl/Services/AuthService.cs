using System.Security.Cryptography;
using B2B.Dao.Repositories.Interfaces;
using B2B.Domain;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService) : IAuthService
{
    private const int MinPbkdf2Iterations = 100_000;
    private const int ExpectedPbkdf2Parts = 4;

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

        return LoginResultDomain.Failed("帳號或密碼錯誤");
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
        var parts = storedPasswordHash.Split(':', ExpectedPbkdf2Parts);

        if (parts.Length != ExpectedPbkdf2Parts ||
            !string.Equals(parts[0], "PBKDF2-SHA256", StringComparison.Ordinal) ||
            !int.TryParse(parts[1], out var iterations) ||
            iterations < MinPbkdf2Iterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
