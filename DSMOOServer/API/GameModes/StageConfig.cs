namespace DSMOOServer.API.GameModes;

public class StageConfig
{
    public string Name { get; set; } = "";
    public string[] StartingStage { get; set; } = [];
    public bool AllOnSameStartingStage { get; set; } = true;
    public string[] WaitingStage { get; set; } = [];
    public bool AllOnSameWaitingStage { get; set; } = true;
    public string[] AllowedStages { get; set; } = [];
    public bool AllowAll { get; set; } = false;
}