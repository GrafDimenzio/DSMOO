using DSMOOFramework.Events;
using DSMOOServer.API.GameModes;
using DSMOOServer.API.GameModes.Hints;

namespace DSMOOServer.API.Events.Args;

public class GetHintEventArgs : IEventArg
{
    public IGame? Game { get; init; }
    public HintData HintData { get; set; }
}