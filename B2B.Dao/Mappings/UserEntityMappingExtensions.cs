using B2B.Dao.Entities;
using B2B.Domain;

namespace B2B.Dao.Mappings;

/// <summary>
/// 提供使用者資料實體與領域模型的轉換方法。
/// </summary>
internal static class UserEntityMappingExtensions
{
    /// <summary>
    /// 將使用者資料實體轉換為領域模型。
    /// </summary>
    /// <param name="entity">使用者資料實體。</param>
    /// <returns>使用者領域模型。</returns>
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
