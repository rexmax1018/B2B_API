using B2B.Dao.Entities;
using B2B.Domain;

namespace B2B.Dao.Mappings;

internal static class UserEntityMappingExtensions
{
    public static UserDomain ToModel(this UserEntity entity) => new()
    {
        UserId = entity.UserId,
        Account = entity.Account,
        DisplayName = entity.DisplayName,
        PasswordHash = entity.PasswordHash,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };
}
