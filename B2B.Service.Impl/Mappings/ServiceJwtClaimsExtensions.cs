using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using B2B.Domain;

namespace B2B.Service.Impl.Mappings;

/// <summary>
/// 提供服務身分轉換為 JWT Claims 的方法。
/// </summary>
public static class ServiceJwtClaimsExtensions
{
    /// <summary>
    /// 將服務身分轉換為 JWT Claims。
    /// </summary>
    /// <param name="service">服務身分資料。</param>
    /// <returns>JWT Claims 集合。</returns>
    public static IEnumerable<Claim> ToJwtClaims(this ServiceDomain service)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, service.ServiceId);
        yield return new Claim("service_name", service.ServiceName);
    }
}
