using System.Security.Cryptography;

namespace B2B_Conn;

internal sealed class RsaPrivateKeyDecryptor
{
    public byte[] Decrypt(byte[] encrypted, RsaKeyModel keyModel)
    {
        if (string.IsNullOrWhiteSpace(keyModel?.PrivateKey))
        {
            throw new InvalidOperationException("RSA 私鑰內容不可空白。");
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyModel.PrivateKey);

        return rsa.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);
    }
}
