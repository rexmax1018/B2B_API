using B2B.CryptoLib;
using B2B.CryptoLib.Models;
using Microsoft.Extensions.Configuration;

namespace B2B.Dao.Extensions;

/// <summary>
/// 提供 B2B API 的 CryptoLib 啟動設定。
/// </summary>
public static class CryptoConfigurationExtensions
{
    private const string EnabledKey = "Crypto:Enabled";
    private const string KeyManagerBasePathKey = "Crypto:KeyManagerBasePath";
    private const string ActiveUnifiedNameKey = "Crypto:ActiveUnifiedName";

    /// <summary>
    /// 依宿主內容根目錄初始化 CryptoLib 的 process default client。
    /// </summary>
    /// <param name="configuration">應用程式設定。</param>
    /// <param name="contentRootPath">宿主的內容根目錄。</param>
    public static void InitializeB2BCrypto(
        this IConfiguration configuration,
        string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (!bool.TryParse(configuration[EnabledKey], out var enabled) || !enabled)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        var configuredBasePath = configuration[KeyManagerBasePathKey];

        if (string.IsNullOrWhiteSpace(configuredBasePath))
        {
            throw new InvalidOperationException(
                $"啟用 Crypto 時必須設定 {KeyManagerBasePathKey}。");
        }

        var activeUnifiedName = configuration[ActiveUnifiedNameKey];

        if (string.IsNullOrWhiteSpace(activeUnifiedName))
        {
            throw new InvalidOperationException(
                $"啟用 Crypto 時必須設定 {ActiveUnifiedNameKey}。");
        }

        var keyManagerBasePath = Path.IsPathFullyQualified(configuredBasePath)
            ? configuredBasePath
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredBasePath));

        Crypto.Initialize(new CryptoOptions
        {
            KeyManagerBasePath = keyManagerBasePath,
            ActiveUnifiedName = activeUnifiedName
        });
    }
}
