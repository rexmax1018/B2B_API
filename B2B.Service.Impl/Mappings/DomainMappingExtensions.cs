using System.Security.Claims;
using B2B.Domain;

namespace B2B.Service.Impl.Mappings;

public static class DomainMappingExtensions
{
    public static IEnumerable<Claim> ToJwtClaims(this UserDomain user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString());
        yield return new Claim(ClaimTypes.Name, user.DisplayName);
        yield return new Claim("account", user.Account);
    }
}
