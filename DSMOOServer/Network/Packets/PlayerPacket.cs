using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DSMOOServer.Network.Packets;

[Packet(PacketType.Player)]
public struct PlayerPacket : IPacket, IEquatable<PlayerPacket>
{
    public const int ActSize = 0x20;
    public const int SubActSize = 0x10;

    public Vector3 Position;
    public Quaternion Rotation;

    public float[] AnimationBlendWeights = [];

    public ushort Act;
    public ushort SubAct;

    public PlayerPacket()
    {
        Position = default;
        Rotation = default;
        Act = 0;
        SubAct = 0;
    }

    public short Size => 0x38;

    public void Serialize(Span<byte> data)
    {
        var offset = 0;
        MemoryMarshal.Write(data[..(offset += Marshal.SizeOf<Vector3>())], Position);
        MemoryMarshal.Write(data[offset..(offset += Marshal.SizeOf<Quaternion>())], Rotation);
        AnimationBlendWeights.CopyTo(MemoryMarshal.Cast<byte, float>(data[offset..(offset += 4 * 6)]));
        MemoryMarshal.Write(data[offset++..++offset], Act);
        MemoryMarshal.Write(data[offset++..++offset], SubAct);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        var offset = 0;
        Position = MemoryMarshal.Read<Vector3>(data[..(offset += Marshal.SizeOf<Vector3>())]);
        Rotation = MemoryMarshal.Read<Quaternion>(data[offset..(offset += Marshal.SizeOf<Quaternion>())]);
        AnimationBlendWeights = MemoryMarshal.Cast<byte, float>(data[offset..(offset += 4 * 6)]).ToArray();
        Act = MemoryMarshal.Read<ushort>(data[offset++..++offset]);
        SubAct = MemoryMarshal.Read<ushort>(data[offset++..++offset]);
    }

    public bool Equals(PlayerPacket other)
    {
        return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && AnimationBlendWeights.Equals(other.AnimationBlendWeights) && Act == other.Act && SubAct == other.SubAct;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Rotation, AnimationBlendWeights, Act, SubAct);
    }
}