using DSMOOFramework.Events;
using DSMOOServer.API.Player;
using DSMOOServer.Commands;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Events.Args;

public class PlayerStateEventArgs : IEventArg
{
    public required IPlayer Player { get; init; }
    public PlayerPacket Packet { get; set; }
    public bool Invisible { get; set; } = false;
    public List<Guid> SpecificInvisible { get; set; } = [];
    public Dictionary<Guid, PlayerPacket> SpecificPackets { get; set; } = [];
}