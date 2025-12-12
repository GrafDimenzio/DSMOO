using DSMOOFramework.Events;
using DSMOOServer.API.Player;
using DSMOOServer.Network;

namespace DSMOOServer.API.Events.Args;

public class DummySendPacketEventArgs(Dummy dummy, PacketHeader packetHeader, IPacket packet) : IEventArg
{
    public Dummy Dummy { get; } = dummy;

    public PacketHeader Header { get; } = packetHeader;

    public IPacket Packet { get; } = packet;

    public IPacket? ReplacePacket { get; set; } = null;

    public Dictionary<Guid, IPacket> SpecificReplacePackets { get; } = new();

    public Type PacketType => Packet.GetType();

    public bool Broadcast { get; set; } = true;
}