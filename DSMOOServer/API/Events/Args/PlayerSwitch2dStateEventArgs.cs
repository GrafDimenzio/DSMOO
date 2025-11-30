using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerSwitch2dStateEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public bool IsNow2d { get; init; }
}