using DSMOOFramework.Events;
using DSMOOServer.Connection;
using DSMOOServer.Network;

namespace DSMOOServer.API.Events.Args;

public class SendPlayerInitPacketEventArgs(IPacket packet, Client client) : IEventArg
{
    public IPacket Packet { get; set; } = packet;
    public Client Client { get; } = client;
}