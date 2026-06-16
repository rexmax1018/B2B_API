using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using B2B.Domain;
using B2B.Service.Impl.Mappings;
using B2B.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace B2B.Service.Impl.Services;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    private const string RefreshTokenType = "refresh";
    private const string TokenTypeClaim = "token_type";

    public TokenDomain GenerateToken(UserDomain user)
    {
        var jwt = ReadJwtSettings();
        var now = DateTime.UtcNow;
        var accessExpiresAt = now.AddMinutes(jwt.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(jwt.RefreshTokenDays);

        var accessToken = GenerateJwt(user.ToJwtClaims(), jwt.Audience, accessExpiresAt, now, jwt);
        var refreshToken = GenerateJwt(CreateRefreshTokenClaims(user), GetRefreshTokenAudience(jwt), refreshExpiresAt, now, jwt);

        return new TokenDomain
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = (int)TimeSpan.FromMinutes(jwt.AccessTokenMinutes).TotalSeconds,
            AccessTokenExpiresAt = accessExpiresAt
        };
    }

    public long? GetRefreshTokenUserId(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var jwt = ReadJwtSettings();

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                refreshToken,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = GetRefreshTokenAudience(jwt),
                    IssuerSigningKey = CreateSigningKey(jwt),
                    ClockSkew = TimeSpan.FromMinutes(1)
                },
                out _);

            if (!string.Equals(principal.FindFirst(TokenTypeClaim)?.Value, RefreshTokenType, StringComparison.Ordinal))
            {
                return null;
            }

            var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return long.TryParse(userIdValue, out var userId) ? userId : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static IEnumerable<Claim> CreateRefreshTokenClaims(UserDomain user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString());
        yield return new Claim(TokenTypeClaim, RefreshTokenType);
        yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"));
    }

    private static string GenerateJwt(
        IEnumerable<Claim> claims,
        string audience,
        DateTime expiresAt,
        DateTime now,
        JwtSettings jwt)
    {
        var credentials = new SigningCredentials(CreateSigningKey(jwt), SecurityAlgorithms.HmacSha256);
        var securityToken = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    private static SymmetricSecurityKey CreateSigningKey(JwtSettings jwt) =>
        new(Encoding.UTF8.GetBytes(jwt.SecretKey));

    private static string GetRefreshTokenAudience(JwtSettings jwt) => $"{jwt.Audience}:refresh";

    private JwtSettings ReadJwtSettings()
    {
        var section = configuration.GetSection("Jwt");

        return new JwtSettings
        {
            Issuer = section["Issuer"] ?? "B2B_API",
            Audience = section["Audience"] ?? "B2B_API_CLIENT",
            SecretKey = section["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is required."),
            AccessTokenMinutes = ReadInt(section["AccessTokenMinutes"], 60),
            RefreshTokenDays = ReadInt(section["RefreshTokenDays"], 7)
        };
    }

    private static int ReadInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) ? parsedValue : defaultValue;
    }

    private sealed class JwtSettings
    {
        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public string SecretKey { get; init; } = string.Empty;

        public int AccessTokenMinutes { get; init; }

        public int RefreshTokenDays { get; init; }
    }
}
