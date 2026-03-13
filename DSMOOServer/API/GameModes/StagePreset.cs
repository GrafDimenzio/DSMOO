namespace DSMOOServer.API.GameModes;

public class StagePreset
{
    public string Name { get; set; } = "";
    public string[] StartingStages { get; set; } = [];
    public bool AllOnSameStartingStage { get; set; } = true;
    public string[] WaitingStages { get; set; } = [];
    public bool AllOnSameWaitingStage { get; set; } = true;
    public string[] AllowedStages { get; set; } = [];
    public bool AllowAll { get; set; } = false;
}