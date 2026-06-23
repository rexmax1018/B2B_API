using System.Text;

namespace B2B_Conn;

internal sealed class SymmetricKeyProvider
{
    private readonly KeySetProvider keySetProvider;
    private readonly RsaPrivateKeyDecryptor rsaPrivateKeyDecryptor;

    public SymmetricKeyProvider(KeySetProvider keySetProvider, RsaPrivateKeyDecryptor rsaPrivateKeyDecryptor)
    {
        this.keySetProvider = keySetProvider;
        this.rsaPrivateKeyDecryptor = rsaPrivateKeyDecryptor;
    }

    public SymmetricKeyModel GetKey()
    {
        var keySetInfo = keySetProvider.GetLatestActiveKeyGroup();
        var encryptedAesKeyIvBytes = File.ReadAllBytes(keySetInfo.AesPath);
        var rsaKeyModel = new RsaKeyModel
        {
            PublicKey = File.ReadAllText(keySetInfo.RsaPublicKeyPath, Encoding.UTF8),
            PrivateKey = File.ReadAllText(keySetInfo.RsaPrivateKeyPath, Encoding.UTF8)
        };

        return ParseEncryptedSymmetricKey(encryptedAesKeyIvBytes, rsaKeyModel);
    }

    private SymmetricKeyModel ParseEncryptedSymmetricKey(byte[] encryptedBytes, RsaKeyModel rsaKey)
    {
        try
        {
            var decrypted = rsaPrivateKeyDecryptor.Decrypt(encryptedBytes, rsaKey);
            var text = Encoding.UTF8.GetString(decrypted);
            var parts = text.Split(':');

            if (parts.Length != 2)
            {
                throw new FormatException("SymmetricKey 內容必須為 Base64Key:Base64IV。");
            }

            return new SymmetricKeyModel
            {
                Key = Convert.FromBase64String(parts[0]),
                IV = Convert.FromBase64String(parts[1])
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("解密加密的 SymmetricKey (AES-Key+IV) 時發生錯誤。", ex);
        }
    }
}
