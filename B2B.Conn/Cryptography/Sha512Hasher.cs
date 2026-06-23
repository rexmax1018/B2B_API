using System.Security.Cryptography;
using System.Text;

namespace B2B_Conn;

internal static class Sha512Hasher
{
    public static string Hash(string? value)
    {
        var input = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var output = SHA512.HashData(input);

        return Convert.ToBase64String(output);
    }
}
