using System.Text;

namespace B2B_Conn;

internal sealed class IniCredentialStore
{
    private readonly B2BConnOptions options;

    public IniCredentialStore(B2BConnOptions options)
    {
        this.options = options;
    }

    public Dictionary<string, string> Load(string path)
    {
        var encryptedContent = LoadIniFile(path);
        if (string.IsNullOrEmpty(encryptedContent))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var decryptedContent = encryptedContent.DecryptAES(options);
        return CredentialTextParser.Parse(decryptedContent);
    }

    private static string LoadIniFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return string.Empty;
        }

        var content = new StringBuilder();

        using var streamReader = File.OpenText(path);
        string? line;
        while ((line = streamReader.ReadLine()) != null)
        {
            content.Append(line.Trim());
        }

        return content.ToString();
    }
}
