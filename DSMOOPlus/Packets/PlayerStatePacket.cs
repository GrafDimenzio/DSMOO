using System.Runtime.InteropServices;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(17)] //Health_Coins
public struct PlayerStatePacket : IPacket
{
    public byte Health { get; set; }
    public int Coins { get; set; }
    public bool Kill { get; set; }

    public short Size => sizeof(byte) + sizeof(int) + sizeof(bool);
    
    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, Health);
        MemoryMarshal.Write(data[sizeof(byte)..], Coins);
        MemoryMarshal.Write(data[(sizeof(byte) + sizeof(int))..], Kill);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Health = MemoryMarshal.Read<byte>(data);
        Coins = MemoryMarshal.Read<int>(data[sizeof(byte)..]);
        Kill = MemoryMarshal.Read<bool>(data[(sizeof(byte) + sizeof(int))..]);
    }
}