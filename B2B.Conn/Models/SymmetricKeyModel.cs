namespace B2B_Conn;

internal class SymmetricKeyModel
{
    public byte[] Key { get; set; } = Array.Empty<byte>();

    public byte[] IV { get; set; } = Array.Empty<byte>();
}
