using System.Runtime.InteropServices;

namespace DSMOOServer.Network;

[StructLayout(LayoutKind.Sequential)]
public struct PacketHeader : IPacket
{
    public Guid Id;
    public short Type;
    public short PacketSize;
    
    public short Size => Constants.HeaderSize;

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data[..16], Id);
        MemoryMarshal.Write(data[16..], Type);
        MemoryMarshal.Write(data[18..], PacketSize);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        Id = MemoryMarshal.Read<Guid>(data[..16]);
        Type = MemoryMarshal.Read<short>(data[16..]);
        PacketSize = MemoryMarshal.Read<short>(data[18..]);
    }
}