using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    [StringLength(4096, MinimumLength = 20)]
    public string RefreshToken { get; set; } = string.Empty;
}
