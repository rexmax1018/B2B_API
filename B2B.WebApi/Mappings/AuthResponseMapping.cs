using B2B.Domain;
using B2B.WebApi.Model.Auth;

namespace B2B.WebApi.Mappings;

internal static class AuthResponseMapping
{
    public static LoginResponse ToLoginResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };

    public static RefreshTokenResponse ToRefreshTokenResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };
}
