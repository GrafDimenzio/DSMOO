namespace DSMOOServer.API.Stage;

public class StageInfo
{
    public string StageName { get; set; } = "";
    public string? Alias { get; set; }
    public WarpInfo[] Warps { get; set; } = [];
}