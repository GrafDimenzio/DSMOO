using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerAddComponentsEventArgs : IEventArg
{
    public required IPlayer Player { get; init; }
    public bool IsDummy => Player.IsDummy;
}