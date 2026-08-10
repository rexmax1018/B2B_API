using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登入 API 的請求內容。
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// 取得或設定由其他專案傳入的 AES 加密 Entry 憑證。
    /// </summary>
    [Required]
    [StringLength(4096, MinimumLength = 29)]
    public string EncryptedCredential { get; set; } = string.Empty;
}
