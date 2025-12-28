using System.Runtime.InteropServices;
using System.Text;
using DSMOOServer;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(PacketType.Init)]
public struct InitPacket : IPacket
{
    public const int VersionSize = 0x20;
    
    public short Size { get; } = sizeof(ushort) + VersionSize;
    public ushort MaxPlayers = 0;
    public string Version { get; set; } = "DSMOOPlus 0.1 pre";

    public InitPacket() { }

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, MaxPlayers);
        Encoding.ASCII.GetBytes(Version).CopyTo(data[sizeof(ushort)..]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        MaxPlayers = MemoryMarshal.Read<ushort>(data);
        Version = Encoding.ASCII.GetString(data[sizeof(ushort)..]).TrimNullTerm();
    }
}