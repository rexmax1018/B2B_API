namespace B2B.Domain;

public sealed class UserDomain
{
    public long UserId { get; set; }

    public string Account { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
