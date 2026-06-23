using System.Globalization;

namespace B2B_Conn;

internal sealed class CredentialResolutionService
{
    private const string IniFilePrefix = "B2BConn";
    private readonly B2BConnOptions options;
    private readonly TimeProvider timeProvider;
    private readonly ConnectionProfileProvider connectionProfileProvider;
    private readonly IniCredentialStore iniCredentialStore;

    public CredentialResolutionService(
        B2BConnOptions options,
        TimeProvider timeProvider,
        ConnectionProfileProvider connectionProfileProvider,
        IniCredentialStore iniCredentialStore)
    {
        this.options = options;
        this.timeProvider = timeProvider;
        this.connectionProfileProvider = connectionProfileProvider;
        this.iniCredentialStore = iniCredentialStore;
    }

    public Entity_Connection Resolve(string? envType, string? svrType, string? dbType, string? accType)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var normalizedEnvType = TextNormalizer.Normalize(envType);
        var normalizedSvrType = TextNormalizer.Normalize(svrType);
        var normalizedDBType = TextNormalizer.Normalize(dbType);
        var normalizedAccType = TextNormalizer.Normalize(accType);

        if (string.IsNullOrEmpty(normalizedEnvType) ||
            string.IsNullOrEmpty(normalizedSvrType) ||
            string.IsNullOrEmpty(normalizedDBType) ||
            string.IsNullOrEmpty(normalizedAccType))
        {
            return new Entity_Connection();
        }

        var connection = connectionProfileProvider.Find(normalizedEnvType, normalizedSvrType, normalizedDBType);
        if (connection is null || string.IsNullOrEmpty(connection.DataSource))
        {
            return new Entity_Connection();
        }

        var suffix = MonthCredentialSelector.GetMonthlySuffix(now.Month);
        var effectiveAccType = $"{normalizedAccType}{suffix}";
        var fileName = $"{IniFilePrefix}{suffix}.ini";
        var iniPath = string.IsNullOrWhiteSpace(connection.IniPath)
            ? options.BasePath
            : connection.IniPath;
        var passwordPrefixes = iniCredentialStore.Load(Path.Combine(iniPath, fileName));

        if (!passwordPrefixes.TryGetValue(effectiveAccType, out var passwordPrefix))
        {
            return new Entity_Connection();
        }

        var password = PasswordFormatter.Format(passwordPrefix, now.Year, now.Month);
        if (string.IsNullOrEmpty(password))
        {
            return new Entity_Connection();
        }

        return new Entity_Connection
        {
            DataSource = connection.DataSource,
            Acc = effectiveAccType,
            pwd = password
        };
    }
}
