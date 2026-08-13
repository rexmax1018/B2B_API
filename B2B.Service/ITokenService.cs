using B2B.Domain;

namespace B2B.Service.Interfaces;

/// <summary>
/// 定義權杖簽發服務。
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 依服務身分產生 Access Token 與 Refresh Token。
    /// </summary>
    /// <param name="service">服務身分資料。</param>
    /// <returns>簽發的權杖資料。</returns>
    TokenDomain GenerateToken(ServiceDomain service);
}
