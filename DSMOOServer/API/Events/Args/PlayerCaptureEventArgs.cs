using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerCaptureEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public string Capture { get; init; }
}