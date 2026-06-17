using System.Security.Claims;
using B2B.Domain;

namespace B2B.Service.Impl.Mappings;

/// <summary>
/// 提供領域模型轉換為驗證資料的方法。
/// </summary>
public static class DomainMappingExtensions
{
    /// <summary>
    /// 將使用者資料轉換為 JWT Claims。
    /// </summary>
    /// <param name="user">使用者資料。</param>
    /// <returns>JWT Claims 集合。</returns>
    public static IEnumerable<Claim> ToJwtClaims(this UserDomain user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString());
        yield return new Claim(ClaimTypes.Name, user.DisplayName);
        yield return new Claim("account", user.Account);
    }
}
