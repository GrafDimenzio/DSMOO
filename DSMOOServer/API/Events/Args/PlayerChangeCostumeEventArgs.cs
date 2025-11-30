using DSMOOFramework.Events;
using DSMOOServer.API.Player;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Events.Args;

public class PlayerChangeCostumeEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public CostumePacket OldCostume { get; init; }
    public CostumePacket NewCostume { get; init; }
}