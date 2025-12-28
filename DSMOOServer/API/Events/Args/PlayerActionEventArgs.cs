using DSMOOFramework.Events;
using DSMOOServer.API.Enum;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerActionEventArgs : IEventArg
{
    public required IPlayer Player { get; init; }
    public required PlayerAction Action { get; init; }
}