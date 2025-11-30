using System.Runtime.InteropServices;
using DSMOOServer.Network;

namespace DSMOOServer.API.Recording;

[StructLayout(LayoutKind.Sequential)]
public struct RecordingHeader {
    public int Timestamp;
    public PacketType Type;

    public static short StaticSize => 6;
    public short Size => StaticSize;

    public void Serialize(Span<byte> data) {
        MemoryMarshal.Write(data[..4], ref Timestamp);
        MemoryMarshal.Write(data[4..], ref Type);
    }

    public void Deserialize(ReadOnlySpan<byte> data) {
        Timestamp = MemoryMarshal.Read<int>(data[..4]);
        Type = MemoryMarshal.Read<PacketType>(data[4..]);
    }
}