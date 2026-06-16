using B2B.Domain;

namespace B2B.Service.Interfaces;

public interface ITokenService
{
    TokenDomain GenerateToken(UserDomain user);

    long? GetRefreshTokenUserId(string refreshToken);
}
