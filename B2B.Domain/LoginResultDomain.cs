namespace B2B.Domain;

public sealed class LoginResultDomain
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public UserDomain? User { get; set; }

    public TokenDomain? Token { get; set; }

    public static LoginResultDomain Failed(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static LoginResultDomain Succeeded(UserDomain user, TokenDomain token) => new()
    {
        Success = true,
        User = user,
        Token = token
    };
}
