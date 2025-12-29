using System.Text;
using DSMOOServer;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(19)]
public struct ChangeCostumePacket : IPacket
{
    public string Body { get; set; }
    public string Cap { get; set; }

    public short Size => Constants.CostumeNameSize * 2;
    
    public void Serialize(Span<byte> data)
    {
        Encoding.ASCII.GetBytes(Body).CopyTo(data[..Constants.CostumeNameSize]);
        Encoding.ASCII.GetBytes(Cap).CopyTo(data[Constants.CostumeNameSize..]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Body = Encoding.ASCII.GetString(data[..Constants.CostumeNameSize]).TrimNullTerm();
        Cap = Encoding.ASCII.GetString(data[Constants.CostumeNameSize..]).TrimNullTerm();
    }
}