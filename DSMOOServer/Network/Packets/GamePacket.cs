using System.Runtime.InteropServices;
using System.Text;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.Game)]
public struct GamePacket : IPacket
{
    public bool Is2d = false;
    public byte ScenarioNum = 0;
    public string Stage = "";

    public GamePacket()
    {
    }

    public short Size => sizeof(bool) + sizeof(byte) + Constants.GamePacketStageSize;//0x42;

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, Is2d);
        MemoryMarshal.Write(data[1..], ScenarioNum);
        Encoding.UTF8.GetBytes(Stage).CopyTo(data[2..(2 + Constants.GamePacketStageSize)]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Is2d = MemoryMarshal.Read<bool>(data);
        ScenarioNum = MemoryMarshal.Read<byte>(data[1..]);
        Stage = Encoding.UTF8.GetString(data[2..(2 + Constants.GamePacketStageSize)]).TrimEnd('\0');
    }
}