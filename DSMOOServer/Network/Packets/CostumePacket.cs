using System.Text;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.Costume)]
public struct CostumePacket : IPacket
{
    public string BodyName;
    public string CapName;

    public short Size => Constants.CostumeNameSize * 2;

    public void Serialize(Span<byte> data)
    {
        Encoding.ASCII.GetBytes(BodyName).CopyTo(data[..Constants.CostumeNameSize]);
        Encoding.ASCII.GetBytes(CapName).CopyTo(data[Constants.CostumeNameSize..]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        BodyName = Encoding.ASCII.GetString(data[..Constants.CostumeNameSize]).TrimNullTerm();
        CapName = Encoding.ASCII.GetString(data[Constants.CostumeNameSize..]).TrimNullTerm();
    }
}