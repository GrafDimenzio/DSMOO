using DSMOOFramework.Events;
using DSMOOServer.API.GameModes;

namespace DSMOOServer.API.Events.Args;

public class WaitingGameEventArgs : IEventArg
{
    public WaitingGame Game { get; init; }
}