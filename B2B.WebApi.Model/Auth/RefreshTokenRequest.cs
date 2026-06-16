using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
