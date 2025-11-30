using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerCollectMoonEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public int Moon { get; set; }
}