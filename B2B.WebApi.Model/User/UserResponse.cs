namespace B2B.WebApi.Model.User;

/// <summary>
/// 表示可安全回傳給 API 呼叫端的使用者資料。
/// </summary>
public sealed class UserResponse
{
    public long UserId { get; set; }
    public string Account { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
