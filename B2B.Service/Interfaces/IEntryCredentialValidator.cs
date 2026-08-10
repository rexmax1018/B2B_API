namespace B2B.Service.Interfaces;

/// <summary>
/// 驗證其他專案傳入的加密 Entry 憑證。
/// </summary>
public interface IEntryCredentialValidator
{
    /// <summary>
    /// 取得目前載入的憑證是否為公開開發範例。
    /// </summary>
    bool IsDevelopmentFixture { get; }

    /// <summary>
    /// 判斷傳入的 AES 加密憑證是否與本機 Entry.ini 相符。
    /// </summary>
    /// <param name="encryptedCredential">傳入的 AES-GCM 密文。</param>
    /// <returns>相符時為 <see langword="true"/>。</returns>
    bool IsValid(string? encryptedCredential);
}
