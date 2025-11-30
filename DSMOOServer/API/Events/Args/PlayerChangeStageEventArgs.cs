using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerChangeStageEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public string PreviousStage { get; init; }
    public string NewStage { get; init; }
    public int PreviousScenario { get; init; }
    public int NewScenario { get; init; }
}