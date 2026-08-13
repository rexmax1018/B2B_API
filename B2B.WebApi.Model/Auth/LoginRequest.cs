using System.ComponentModel.DataAnnotations;

namespace B2B.WebApi.Model.Auth;

/// <summary>
/// 表示登入 API 的請求內容。
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// 取得或設定由呼叫端傳入的應用程式憑證。
    /// </summary>
    [Required]
    [StringLength(4096)]
    public string Credential { get; set; } = string.Empty;
}
