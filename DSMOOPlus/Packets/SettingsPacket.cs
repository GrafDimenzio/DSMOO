using System.Runtime.InteropServices;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(16)] //Extra Packet
public struct SettingsPacket : IPacket
{
    public bool InfiniteCapBounce { get; set; }
    public bool Noclip { get; set; }

    public short Size => sizeof(bool) + sizeof(bool);
    
    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, InfiniteCapBounce);
        MemoryMarshal.Write(data[sizeof(bool)..], Noclip);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        InfiniteCapBounce = MemoryMarshal.Read<bool>(data);
        Noclip = MemoryMarshal.Read<bool>(data[sizeof(bool)..]);
    }
}