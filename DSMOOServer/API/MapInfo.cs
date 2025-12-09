using System.Reflection;
using System.Text.Json;

namespace DSMOOServer.API;

public class MapInfo
{
  static MapInfo()
  {
    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DSMOOServer.kingdoms.json");
    using var reader = new StreamReader(stream!);
    var json = reader.ReadToEnd();
    AllKingdoms = JsonSerializer.Deserialize<MapInfo[]>(json)!;
  }

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
  
  public static readonly MapInfo[] AllKingdoms;
  
  public string KingdomName { get; set; } = "";
  public string MainStageName { get; set; } = "";
  public float Rotation { get; set; } = 0;
  public OffsetObject Offset { get; set; } = new OffsetObject();
  public float Scale { get; set; } = 1;
  public SubAreaObject[] SubAreas { get; set; } = [];
  
  public class OffsetObject
  {
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;
  }
  
  public class SubAreaObject
  {
    public string SubAreaName { get; set; } = "";
    public OffsetObject Position { get; set; } = new OffsetObject();
    public string Entrance { get; set; } = "";
    public string Exit { get; set; } = "";
  }
}