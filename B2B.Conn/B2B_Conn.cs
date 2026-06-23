namespace B2B_Conn;

public class B2B_Conn : IDisposable
{
    private readonly ConnectionProfileProvider connectionProfileProvider;
    private readonly CredentialResolutionService credentialResolutionService;
    private bool disposed;

    public B2B_Conn()
        : this(B2BConnOptions.Default)
    {
    }

    public B2B_Conn(B2BConnOptions options)
        : this(options, TimeProvider.System)
    {
    }

    internal B2B_Conn(B2BConnOptions options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        connectionProfileProvider = new ConnectionProfileProvider(options);
        credentialResolutionService = new CredentialResolutionService(
            options,
            timeProvider,
            connectionProfileProvider,
            new IniCredentialStore(options));
    }

    internal B2B_Conn(
        ConnectionProfileProvider connectionProfileProvider,
        CredentialResolutionService credentialResolutionService)
    {
        this.connectionProfileProvider = connectionProfileProvider ?? throw new ArgumentNullException(nameof(connectionProfileProvider));
        this.credentialResolutionService = credentialResolutionService ?? throw new ArgumentNullException(nameof(credentialResolutionService));
    }

    ~B2B_Conn()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing) { }

        disposed = true;
    }

    /// <summary>
    /// 釋放實體物件
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string CommConnString(string? EnvType, string? SvrType, string? DBType, string? AccType)
    {
        return OracleConnectionStringFormatter.Format(GetEntityInfo(EnvType, SvrType, DBType, AccType));
    }

    public Entity_Connection GetEntityInfo(string? EnvType, string? SvrType, string? DBType, string? AccType)
    {
        return credentialResolutionService.Resolve(EnvType, SvrType, DBType, AccType);
    }

    [Obsolete("Connection data is now cached in memory. This method is kept only for compatibility with the restored legacy implementation.")]
    private List<B2B_Connection> InitB2BConnection()
    {
        return connectionProfileProvider.GetAll();
    }
}
