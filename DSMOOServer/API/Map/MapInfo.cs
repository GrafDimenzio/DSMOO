using System.Reflection;
using System.Text.Json;
using DSMOOServer.API.Serialized;

namespace DSMOOServer.API.Map;

public class MapInfo
{
    public static readonly MapInfo[] AllKingdoms;

    static MapInfo()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DSMOOServer.Data.kingdoms.json");
        using var reader = new StreamReader(stream!);
        var json = reader.ReadToEnd();
        AllKingdoms = JsonSerializer.Deserialize<MapInfo[]>(json)!;
    }

    public string KingdomName { get; set; } = "";
    public string MainStageName { get; set; } = "";
    public float Rotation { get; set; } = 0;
    public SerializedVector2 Offset { get; set; } = new();
    public float Scale { get; set; } = 1;
    public SubArea[] SubAreas { get; set; } = [];

    public static string GetConnection(string fromStage, string toStage, bool useExit)
    {
        if (AllKingdoms.Any(x => x.MainStageName == toStage))
        {
            var kingdom = AllKingdoms.First(x => x.MainStageName == toStage);
            var subArea = kingdom.SubAreas.FirstOrDefault(x => x.SubAreaName == fromStage);
            if (subArea == null)
                return "";
            return useExit ? subArea.Exit : subArea.Entrance;
        }

        if (AllKingdoms.Any(x => x.MainStageName == fromStage))
        {
            var kingdom = AllKingdoms.First(x => x.MainStageName == fromStage);
            var subArea = kingdom.SubAreas.FirstOrDefault(x => x.SubAreaName == toStage);
            if (subArea == null)
                return "";
            return useExit ? subArea.Exit : subArea.Entrance;
        }

        return "";
    }
}