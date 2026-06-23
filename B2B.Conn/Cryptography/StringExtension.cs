namespace B2B_Conn;

internal static class StringExtension
{
    internal static string DecryptAES(this string? value)
    {
        return DecryptAES(value, B2BConnOptions.Default);
    }

    internal static string DecryptAES(this string? value, B2BConnOptions options)
    {
        return CreateProtector(options).Decrypt(value);
    }

    internal static string EncryptAES(this string? value)
    {
        return EncryptAES(value, B2BConnOptions.Default);
    }

    internal static string EncryptAES(this string? value, B2BConnOptions options)
    {
        return CreateProtector(options).Encrypt(value);
    }

    public static string EncryptSHA(this string? value)
    {
        return Sha512Hasher.Hash(value);
    }

    private static AesStringProtector CreateProtector(B2BConnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new AesStringProtector(
            new SymmetricKeyProvider(
                new KeySetProvider(options),
                new RsaPrivateKeyDecryptor()));
    }
}
