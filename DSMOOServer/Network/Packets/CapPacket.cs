using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.Cap)]
public struct CapPacket : IPacket
{
    public Vector3 Position;
    public Quaternion Rotation;
    public bool CapOut;
    public string CapAnim;

    public short Size => 0x50;

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, Position);
        MemoryMarshal.Write(data[12..], Rotation);
        MemoryMarshal.Write(data[28..], CapOut);
        Encoding.ASCII.GetBytes(CapAnim).CopyTo(data[32..(32 + Constants.CapAnimationSize)]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Position = MemoryMarshal.Read<Vector3>(data);
        Rotation = MemoryMarshal.Read<Quaternion>(data[12..]);
        CapOut = MemoryMarshal.Read<bool>(data[28..]);
        CapAnim = Encoding.ASCII.GetString(data[32..(32 + Constants.CapAnimationSize)]);
    }
}