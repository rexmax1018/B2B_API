using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登出 API 的請求內容。
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// 取得或設定要撤銷的 Refresh Token。
    /// </summary>
    [Required]
    [StringLength(4096, MinimumLength = 20)]
    public string RefreshToken { get; set; } = string.Empty;
}
