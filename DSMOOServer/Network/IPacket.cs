namespace DSMOOServer.Network;

public interface IPacket {
    short Size { get; }
    void Serialize(Span<byte> data);
    void Deserialize(ReadOnlySpan<byte> data);
}