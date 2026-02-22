using System.Runtime.InteropServices;
using System.Text;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.ChangeStage)]
public struct ChangeStagePacket : IPacket
{
    
    public string Stage = "";
    public string Id = "";
    public sbyte Scenario = 0;
    public byte SubScenarioType = 0;
    public short Size => Constants.StageSize + Constants.WarpIdSize + sizeof(sbyte) + sizeof(byte);//0x44;

    public ChangeStagePacket()
    {
    }

    public void Serialize(Span<byte> data)
    {
        Encoding.UTF8.GetBytes(Stage).CopyTo(data[..Constants.StageSize]);
        Encoding.UTF8.GetBytes(Id).CopyTo(data[Constants.StageSize..(Constants.WarpIdSize + Constants.StageSize)]);
        MemoryMarshal.Write(data[(Constants.WarpIdSize + Constants.StageSize)..(Constants.WarpIdSize + Constants.StageSize + 1)], Scenario);
        MemoryMarshal.Write(data[(Constants.WarpIdSize + Constants.StageSize + 1)..(Constants.WarpIdSize + Constants.StageSize + 2)], SubScenarioType);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Stage = Encoding.UTF8.GetString(data[..Constants.StageSize]).TrimNullTerm();
        Id = Encoding.UTF8.GetString(data[Constants.StageSize..(Constants.WarpIdSize + Constants.StageSize)]).TrimNullTerm();
        Scenario = MemoryMarshal.Read<sbyte>(data[(Constants.WarpIdSize + Constants.StageSize)..(Constants.WarpIdSize + Constants.StageSize + 1)]);
        SubScenarioType = MemoryMarshal.Read<byte>(data[(Constants.WarpIdSize + Constants.StageSize + 1)..(Constants.WarpIdSize + Constants.StageSize + 2)]);
    }
}