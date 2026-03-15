using DSMOOFramework.Events;
using DSMOOServer.API.GameModes;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerJoinGameEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public IGame Game { get; init; }
}