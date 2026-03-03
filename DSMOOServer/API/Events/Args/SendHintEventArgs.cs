using DSMOOFramework.Events;
using DSMOOServer.API.GameModes.Hints;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class SendHintEventArgs : IEventArg
{
    public IPlayer SendToPlayer { get; set; }
    public HintData Hint { get; set; }
}