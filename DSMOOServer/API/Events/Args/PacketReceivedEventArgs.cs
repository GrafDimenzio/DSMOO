using DSMOOFramework.Events;
using DSMOOServer.Connection;
using DSMOOServer.Network;

namespace DSMOOServer.API.Events.Args;

public class PacketReceivedEventArgs(PacketHeader packetHeader, IPacket packet, Client sender) : IEventArg
{
    public PacketHeader Header { get; } = packetHeader;

    public IPacket Packet { get; } = packet;

    public IPacket? ReplacePacket { get; set; } = null;

    public Dictionary<Guid, IPacket> SpecificReplacePackets { get; } = new();

    public Type PacketType => Packet.GetType();

    public Client Sender { get; set; } = sender;

    public bool Broadcast { get; set; } = true;
}