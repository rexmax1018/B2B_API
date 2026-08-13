using B2B.Domain;
using B2B.WebApi.Model.User;

namespace B2B.WebApi.Mappings;

/// <summary>
/// 提供使用者領域模型轉換為 API 回應的方法。
/// </summary>
internal static class UserResponseMapping
{
    /// <summary>
    /// 轉換為不含密碼雜湊的使用者回應。
    /// </summary>
    public static UserResponse ToUserResponse(this UserDomain user)
    {
        // TODO[MIGRATE-RESPONSE]: 將 .NET Framework 4.8 的公開欄位逐一映射到 UserResponse。
        // TODO[MIGRATE-SECURITY]: 確認敏感欄位（例如 PasswordHash）永不加入 API 回應。
        return new UserResponse
        {
            UserId = user.UserId,
            Account = user.Account,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
