namespace B2B_Conn;

public sealed class B2BConnOptions
{
    public static B2BConnOptions Default { get; } = new();

    public string BasePath { get; init; } = @"C:\B2B_Conn\";

    public string CryptoFolderName { get; init; } = "Other";

    public IReadOnlyList<B2B_Connection>? Connections { get; init; }
}
