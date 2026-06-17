using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登入 API 的請求內容。
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// 取得或設定登入帳號。
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定登入密碼。
    /// </summary>
    [Required]
    [StringLength(256, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
