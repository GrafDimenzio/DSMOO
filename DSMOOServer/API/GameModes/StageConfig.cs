namespace DSMOOServer.API.GameModes;

public struct StageConfig
{
    public string Name = "";
    public string[] StartingStage = [];
    public string[] WaitingStage = [];
    public string[] AllowedStages = [];
    public bool AllowAll = false;

    public StageConfig()
    {
    }
}