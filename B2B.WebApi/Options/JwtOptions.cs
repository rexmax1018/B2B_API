using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Options;

/// <summary>
/// 表示 JWT 驗證與簽發設定。
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// JWT 設定區段名稱。
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// 取得或設定 JWT 發行者。
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定 JWT 接收者。
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定 JWT 簽章密鑰。
    /// </summary>
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 取得或設定 Access Token 有效分鐘數。
    /// </summary>
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;

    /// <summary>
    /// 取得或設定 Refresh Token 有效天數。
    /// </summary>
    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 7;
}
