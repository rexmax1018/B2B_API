namespace B2B.Domain;

public sealed class TokenDomain
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public int ExpiresIn { get; set; }

    public DateTime AccessTokenExpiresAt { get; set; }
}
