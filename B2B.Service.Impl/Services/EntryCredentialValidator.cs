using System.Security.Cryptography;
using System.Text;
using B2B.Service.Interfaces;

namespace B2B.Service.Impl.Services;

/// <summary>
/// 從應用程式根目錄載入並驗證 AES-GCM Entry 憑證。
/// </summary>
public sealed class EntryCredentialValidator : IEntryCredentialValidator
{
    /// <summary>
    /// Entry 憑證檔案名稱。
    /// </summary>
    public const string FileName = "Entry.ini";

    private const string AesGcmV1Prefix = "AES-GCM-V1:";
    private const int AesGcmNonceLength = 12;
    private const int AesGcmTagLength = 16;

    // NIST AES-GCM test vector. It is only a tracked development fixture and must never be used in production.
    private const string DevelopmentFixture =
        "AES-GCM-V1:AAAAAAAAAAAAAAAAA4jazmC2o5LzKMK5cbL+eKtuR9Qs7BO99TpnshJXvd8=";

    private readonly byte[] expectedCredential;

    /// <inheritdoc />
    public bool IsDevelopmentFixture { get; }

    /// <summary>
    /// 載入並驗證應用程式根目錄的 Entry.ini。
    /// </summary>
    public EntryCredentialValidator()
    {
        var credentialPath = Path.Combine(AppContext.BaseDirectory, FileName);

        if (!File.Exists(credentialPath))
        {
            throw new FileNotFoundException(
                $"找不到登入憑證檔案：{credentialPath}。請在應用程式根目錄部署 {FileName}。",
                credentialPath);
        }

        var credential = File.ReadAllText(credentialPath, Encoding.UTF8).Trim();
        ValidateCiphertextFormat(credential, credentialPath);

        IsDevelopmentFixture = string.Equals(credential, DevelopmentFixture, StringComparison.Ordinal);
        expectedCredential = Encoding.UTF8.GetBytes(credential);
    }

    /// <inheritdoc />
    public bool IsValid(string? encryptedCredential)
    {
        if (string.IsNullOrWhiteSpace(encryptedCredential))
        {
            return false;
        }

        var candidate = Encoding.UTF8.GetBytes(encryptedCredential.Trim());

        return candidate.Length == expectedCredential.Length &&
            CryptographicOperations.FixedTimeEquals(candidate, expectedCredential);
    }

    /// <summary>
    /// 驗證檔案內容具有 AES-GCM v1 封包外型。
    /// </summary>
    /// <param name="credential">Entry.ini 內容。</param>
    /// <param name="credentialPath">Entry.ini 的完整路徑。</param>
    private static void ValidateCiphertextFormat(string credential, string credentialPath)
    {
        if (!credential.StartsWith(AesGcmV1Prefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{credentialPath} 必須以 {AesGcmV1Prefix} 開頭。");
        }

        try
        {
            var payload = Convert.FromBase64String(credential[AesGcmV1Prefix.Length..]);

            if (payload.Length <= AesGcmNonceLength + AesGcmTagLength)
            {
                throw new InvalidOperationException(
                    $"{credentialPath} 的 AES-GCM payload 不完整。");
            }
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                $"{credentialPath} 的 AES-GCM payload 必須是 Base64。",
                exception);
        }
    }
}
