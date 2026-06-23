using B2B.Service.Options;
using Microsoft.AspNetCore.Builder;

namespace B2B.WebApi.Extensions;

/// <summary>
/// 驗證安全性相關設定是否符合執行環境要求。
/// </summary>
internal static class SecurityConfigurationValidator
{
    private static readonly string PlaceholderJwtSecret =
        string.Concat("PLEASE_CHANGE", "_THIS_SECRET_KEY", "_TO_AT_LEAST_32_CHARS");

    /// <summary>
    /// 驗證應用程式安全性設定。
    /// </summary>
    /// <param name="app">Web 應用程式。</param>
    public static void Validate(WebApplication app)
    {
        ValidateJwtSecret(app.Configuration[$"{JwtOptions.SectionName}:SecretKey"]);

        if (app.Environment.IsDevelopment())
        {
            return;
        }

        RejectEnabled(app.Configuration, "DataAccess:UseFakeRepositories", "非開發環境不可啟用假資料儲存庫。");
        RejectEnabled(app.Configuration, "TransactionLog:IncludeRequestBody", "非開發環境不可記錄請求本文。");
        RejectEnabled(app.Configuration, "TransactionLog:IncludeResponseBody", "非開發環境不可記錄回應本文。");

        var allowedHosts = app.Configuration["AllowedHosts"];

        if (string.IsNullOrWhiteSpace(allowedHosts) || string.Equals(allowedHosts, "*", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("非開發環境必須明確設定 AllowedHosts。");
        }
    }

    /// <summary>
    /// 驗證 JWT 密鑰是否已正確設定。
    /// </summary>
    /// <param name="secret">JWT 密鑰。</param>
    public static void ValidateJwtSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("必須由安全設定來源提供 Jwt:SecretKey。");
        }

        if (string.Equals(secret, PlaceholderJwtSecret, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Jwt:SecretKey 仍使用預設佔位值。");
        }
    }

    /// <summary>
    /// 驗證指定布林設定在目前環境不得啟用。
    /// </summary>
    /// <param name="configuration">應用程式設定。</param>
    /// <param name="key">設定鍵。</param>
    /// <param name="message">啟用時的錯誤訊息。</param>
    private static void RejectEnabled(IConfiguration configuration, string key, string message)
    {
        if (bool.TryParse(configuration[key], out var enabled) && enabled)
        {
            throw new InvalidOperationException(message);
        }
    }
}
