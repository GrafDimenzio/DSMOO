using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOServer.API.Stage;

public class StageManager(ILogger logger) : Manager
{
    public ReadOnlyCollection<StageInfo> Stages { get; private set; } = new([]);
    
    public override void Initialize()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DSMOOServer.Data.stages.json");
        using var reader = new StreamReader(stream!);
        var json = reader.ReadToEnd();
        var stages = JsonSerializer.Deserialize<StageInfo[]>(json)!;
        Stages = new ReadOnlyCollection<StageInfo>(stages);
        
        base.Initialize();
    }
    
    public string GetConnection(string fromStage, string toStage, int index = 0)
    {
        var stage = Stages.FirstOrDefault(x => x.StageName == toStage);
        if (stage == null)
            return "";
        var warp = stage.Warps.Where(x => x.ConnectedStage == fromStage).ToArray();
        return warp.Length == 0 ? "" : warp[Math.Clamp(index, 0, warp.Length - 1)].Name;
    }
}