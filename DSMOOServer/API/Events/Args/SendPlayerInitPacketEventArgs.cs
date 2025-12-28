using DSMOOFramework.Events;
using DSMOOServer.Network;

namespace DSMOOServer.API.Events.Args;

public class SendPlayerInitPacketEventArgs(IPacket packet, Player.Player player) : IEventArg
{
    public IPacket Packet { get; set; } = packet;
    public Player.Player Player { get; } = player;
}