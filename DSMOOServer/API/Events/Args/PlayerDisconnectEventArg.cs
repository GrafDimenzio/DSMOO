using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerDisconnectEventArg : IEventArg
{
    public IPlayer Player { get; init; }
}