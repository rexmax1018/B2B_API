using B2B.CryptoLib;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace B2B.Dao.Mappings;

/// <summary>
/// 提供以 CryptoLib process default client 加密資料庫欄位的 EF Core mapping。
/// </summary>
public static class PropertyBuilderEncryptionExtensions
{
    /// <summary>
    /// 將字串屬性映射為 CryptoLib 的加密字串格式。
    /// </summary>
    /// <remarks>
    /// 此轉換器只負責 storage 與 retrieval：寫入時使用 <see cref="Crypto.Encrypt(string?)" />，
    /// 讀取時使用 <see cref="Crypto.Decrypt(string?)" />。CryptoLib 使用 randomized AES-GCM，
    /// 因此同一明文每次產生的 ciphertext 都不同。
    ///
    /// 此 API 不適合用於 WHERE 等值查詢、LIKE、Contains、StartsWith、EndsWith、JOIN、
    /// 依明文語意排序、唯一明文約束或一般資料庫索引查找。呼叫端仍須自行管理欄位容量；
    /// AES-GCM envelope 與 Base64 會使資料庫儲存長度大於明文長度。NULL 會維持 NULL，
    /// 且本轉換器不提供既有明文 fallback 或自動遷移。
    /// </remarks>
    /// <param name="propertyBuilder">要套用轉換器的字串屬性。</param>
    /// <returns>原屬性建構器，供繼續設定 mapping。</returns>
    public static PropertyBuilder<string> HasB2BEncryption(
        this PropertyBuilder<string> propertyBuilder)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        propertyBuilder.HasConversion(new ValueConverter<string, string>(
            plainText => plainText == null ? null! : Crypto.Encrypt(plainText)!,
            encryptedText => encryptedText == null ? null! : Crypto.Decrypt(encryptedText)!));

        return propertyBuilder;
    }
}
