using System.Security.Cryptography;
using System.Text;

namespace B2B_Conn;

internal sealed class AesStringProtector
{
    private readonly SymmetricKeyProvider symmetricKeyProvider;

    public AesStringProtector(SymmetricKeyProvider symmetricKeyProvider)
    {
        this.symmetricKeyProvider = symmetricKeyProvider;
    }

    public string Decrypt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var aesKey = symmetricKeyProvider.GetKey();
        var encryptedBytes = Convert.FromBase64String(value);
        var decryptedBytes = DecryptBytes(encryptedBytes, aesKey);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public string Encrypt(string? value)
    {
        var aesKey = symmetricKeyProvider.GetKey();
        var plainBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var encryptedBytes = EncryptBytes(plainBytes, aesKey);

        return Convert.ToBase64String(encryptedBytes);
    }

    private static byte[] EncryptBytes(byte[] data, SymmetricKeyModel keyModel)
    {
        ValidateSymmetricKey(keyModel);

        using var aes = Aes.Create();
        aes.Key = keyModel.Key;
        return aes.EncryptCbc(data, keyModel.IV, PaddingMode.PKCS7);
    }

    private static byte[] DecryptBytes(byte[] encrypted, SymmetricKeyModel keyModel)
    {
        ValidateSymmetricKey(keyModel);

        using var aes = Aes.Create();
        aes.Key = keyModel.Key;
        return aes.DecryptCbc(encrypted, keyModel.IV, PaddingMode.PKCS7);
    }

    private static void ValidateSymmetricKey(SymmetricKeyModel keyModel)
    {
        if (keyModel == null || keyModel.Key.Length == 0 || keyModel.IV.Length == 0)
        {
            throw new InvalidOperationException("AES Key 與 IV 不可空白。");
        }
    }
}
