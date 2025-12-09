using DSMOOFramework.Events;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.Events.Args;

public class PlayerChangeStageEventArgs : IEventArg
{
    public IPlayer Player { get; init; }
    public string PreviousStage { get; init; }
    public string NewStage { get; init; }
    public byte PreviousScenario { get; init; }
    public int NewScenario { get; init; }
    public bool SendBack { get; set; } = false;
    public string SendBackStage { get; set; } = "";
    public sbyte SendBackScenario { get; set; } = -1;
    public string SendBackWarp { get; set; } = "";
}