using B2B.Domain;

namespace B2B.Service.Interfaces;

/// <summary>
/// 定義權杖簽發服務。
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 依使用者資料產生 Access Token 與 Refresh Token。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    /// <returns>簽發的權杖資料。</returns>
    TokenDomain GenerateToken(UserDomain user);
}
