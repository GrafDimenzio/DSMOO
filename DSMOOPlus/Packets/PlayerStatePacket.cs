using System.Runtime.InteropServices;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(17)] //Health_Coins
public struct PlayerStatePacket : IPacket
{
    public byte Health = 0;
    public int Coins = 0;
    public bool AssistModeHealth = false;

    public PlayerStatePacket()
    {
    }

    public short Size => sizeof(byte) + sizeof(int) + sizeof(bool);

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, Health);
        MemoryMarshal.Write(data[sizeof(byte)..], Coins);
        MemoryMarshal.Write(data[(sizeof(byte) + sizeof(int))..], AssistModeHealth);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Health = MemoryMarshal.Read<byte>(data);
        Coins = MemoryMarshal.Read<int>(data[sizeof(byte)..]);
        AssistModeHealth = MemoryMarshal.Read<bool>(data[(sizeof(byte) + sizeof(int))..]);
    }
}