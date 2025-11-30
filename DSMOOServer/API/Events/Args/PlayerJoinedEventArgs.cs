using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerJoinedEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
}