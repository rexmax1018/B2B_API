using System.Collections.Frozen;

namespace B2B_Conn;

internal sealed class ConnectionProfileProvider
{
    private readonly FrozenDictionary<(string EnvType, string SvrType, string DBType), B2B_Connection> connectionLookup;

    public ConnectionProfileProvider(B2BConnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var connections = options.Connections ?? DefaultConnectionProfiles.Create(options.BasePath);
        connectionLookup = connections
            .GroupBy(connection => (
                EnvType: TextNormalizer.Normalize(connection.EnvType),
                SvrType: TextNormalizer.Normalize(connection.SvrType),
                DBType: TextNormalizer.Normalize(connection.DBType)))
            .Where(group => !string.IsNullOrEmpty(group.Key.EnvType) &&
                            !string.IsNullOrEmpty(group.Key.SvrType) &&
                            !string.IsNullOrEmpty(group.Key.DBType))
            .ToFrozenDictionary(group => group.Key, group => group.First());
    }

    public B2B_Connection? Find(string envType, string svrType, string dbType)
    {
        if (string.IsNullOrEmpty(envType) || string.IsNullOrEmpty(svrType) || string.IsNullOrEmpty(dbType))
        {
            return null;
        }

        return connectionLookup.GetValueOrDefault((envType, svrType, dbType));
    }

    public List<B2B_Connection> GetAll()
    {
        return connectionLookup.Values.ToList();
    }
}
