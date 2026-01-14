using DSMOOServer.API.Serialized;

namespace DSMOOServer.API.Map;

public class SubArea
{
    public string SubAreaName { get; set; } = "";
    public SerializedVector2 Position { get; set; } = new();
}