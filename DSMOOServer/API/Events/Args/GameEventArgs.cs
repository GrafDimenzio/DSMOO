using DSMOOFramework.Events;
using DSMOOServer.API.GameModes;

namespace DSMOOServer.API.Events.Args;

public class GameEventArgs : IEventArg
{
    public IGame Game { get; init; }
}