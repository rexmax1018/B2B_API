namespace B2B_Conn;

internal class KeySetInfo
{
    public string UnifiedName { get; set; } = string.Empty;

    public string AesPath { get; set; } = string.Empty;

    public string RsaPublicKeyPath { get; set; } = string.Empty;

    public string RsaPrivateKeyPath { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string[] GetAllPaths() => new[] { AesPath, RsaPublicKeyPath, RsaPrivateKeyPath };
}
