using System.Text.RegularExpressions;

namespace B2B_Conn;

internal sealed partial class KeySetProvider
{
    private readonly B2BConnOptions options;

    public KeySetProvider(B2BConnOptions options)
    {
        this.options = options;
    }

    public KeySetInfo GetLatestActiveKeyGroup()
    {
        var cryptoKeyPath = GetCryptoKeyPath();
        if (!Directory.Exists(cryptoKeyPath))
        {
            throw new DirectoryNotFoundException($"找不到 B2B.Conn 金鑰目錄：{cryptoKeyPath}");
        }

        var keySet = Directory.GetFiles(cryptoKeyPath)
            .GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('.')[0])
            .Select(g => new
            {
                UnifiedName = g.Key,
                AesFile = g.FirstOrDefault(f => IsAesKey(Path.GetFileName(f))),
                RsaPublicFile = g.FirstOrDefault(f => IsRsaPublicKey(Path.GetFileName(f))),
                RsaPrivateFile = g.FirstOrDefault(f => IsRsaPrivateKey(Path.GetFileName(f)))
            })
            .Where(g => g.AesFile != null && g.RsaPublicFile != null && g.RsaPrivateFile != null)
            .Select(g => new KeySetInfo
            {
                UnifiedName = g.UnifiedName,
                AesPath = g.AesFile!,
                RsaPublicKeyPath = g.RsaPublicFile!,
                RsaPrivateKeyPath = g.RsaPrivateFile!,
                CreationTime = File.GetCreationTimeUtc(g.AesFile!)
            })
            .OrderByDescending(g => g.CreationTime)
            .FirstOrDefault();

        if (keySet == null)
        {
            throw new FileNotFoundException($"找不到完整的 B2B.Conn 金鑰組：{cryptoKeyPath}");
        }

        return keySet;
    }

    private string GetCryptoKeyPath()
    {
        var basePath = string.IsNullOrWhiteSpace(options.BasePath)
            ? B2BConnOptions.Default.BasePath
            : options.BasePath;
        var cryptoFolderName = string.IsNullOrWhiteSpace(options.CryptoFolderName)
            ? B2BConnOptions.Default.CryptoFolderName
            : options.CryptoFolderName;

        return Path.Combine(basePath, cryptoFolderName);
    }

    private static bool IsAesKey(string fileName) => AesKeyRegex().IsMatch(fileName);

    private static bool IsRsaPublicKey(string fileName) => RsaPublicKeyRegex().IsMatch(fileName);

    private static bool IsRsaPrivateKey(string fileName) => RsaPrivateKeyRegex().IsMatch(fileName);

    [GeneratedRegex(@"^[a-zA-Z0-9]{8}\.der$", RegexOptions.CultureInvariant)]
    private static partial Regex AesKeyRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]{8}\.public\.pem$", RegexOptions.CultureInvariant)]
    private static partial Regex RsaPublicKeyRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]{8}\.private\.pem$", RegexOptions.CultureInvariant)]
    private static partial Regex RsaPrivateKeyRegex();
}
