namespace B2B_Conn;

internal class RsaKeyModel
{
    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public int KeySize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
