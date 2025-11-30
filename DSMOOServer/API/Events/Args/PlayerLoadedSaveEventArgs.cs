using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerLoadedSaveEventArgs(IPlayer player) : IEventArg
{
    public IPlayer Player { get; } = player;
}