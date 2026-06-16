using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
