using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

public sealed class LoginRequest
{
    [Required]
    public string Account { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
