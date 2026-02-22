using System.Runtime.InteropServices;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.Moon)]
public struct MoonPacket : IPacket
{
    public int MoonId;

    public short Size => 4;

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, MoonId);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        MoonId = MemoryMarshal.Read<int>(data);
    }
}