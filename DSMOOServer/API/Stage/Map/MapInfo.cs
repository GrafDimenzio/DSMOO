using DSMOOServer.API.Serialized;

namespace DSMOOServer.API.Stage.Map;

public class MapInfo
{
    public string KingdomName { get; set; } = "";
    public string MainStageName { get; set; } = "";
    public float Rotation { get; set; } = 0;
    public SerializedVector2 Offset { get; set; } = new();
    public float Scale { get; set; } = 1;
    public SubArea[] SubAreas { get; set; } = [];
}