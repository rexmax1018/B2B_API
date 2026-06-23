namespace B2B_Conn;

internal static class CredentialTextParser
{
    public static Dictionary<string, string> Parse(string text)
    {
        var credentials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(text))
        {
            return credentials;
        }

        var credentialItems = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var credentialItem in credentialItems)
        {
            var pair = credentialItem.Split(',', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                continue;
            }

            var account = pair[0];
            var password = pair[1];
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                continue;
            }

            credentials.TryAdd(account, password);
        }

        return credentials;
    }
}
