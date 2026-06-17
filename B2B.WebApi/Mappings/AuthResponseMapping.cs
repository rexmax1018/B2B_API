using B2B.Domain;
using B2B.WebApi.Model.Auth;

namespace B2B.WebApi.Mappings;

/// <summary>
/// 提供權杖領域模型轉換為 API 回應模型的方法。
/// </summary>
internal static class AuthResponseMapping
{
    /// <summary>
    /// 將權杖資料轉換為登入回應。
    /// </summary>
    /// <param name="token">權杖資料。</param>
    /// <returns>登入回應。</returns>
    public static LoginResponse ToLoginResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };

    /// <summary>
    /// 將權杖資料轉換為更新權杖回應。
    /// </summary>
    /// <param name="token">權杖資料。</param>
    /// <returns>更新權杖回應。</returns>
    public static RefreshTokenResponse ToRefreshTokenResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };
}
