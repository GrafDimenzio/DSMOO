using DSMOOServer.API.Serialized;

namespace DSMOOServer.API.Stage;

public class WarpInfo
{
    public string Name { get; set; } = "";
    public string ConnectedStage { get; set; } = "";
    public SerializedVector3 Position { get; set; } = new();
}