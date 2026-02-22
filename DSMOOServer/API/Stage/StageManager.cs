using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Map;
using DSMOOServer.API.Serialized;
using DSMOOServer.Commands;

namespace DSMOOServer.API.Stage;

public class StageManager(ILogger logger) : Manager
{
    private List<StageInfo> _stages = [];
    private List<string> _zones = [];

    public ReadOnlyCollection<StageInfo> Stages => new(_stages);
    public ReadOnlyCollection<string> Zones => new(_zones);
    
    public override void Initialize()
    {
        var json = ReadJson("DSMOOServer.Data.stages.json");
        _stages = JsonSerializer.Deserialize<List<StageInfo>>(json)!;
        
        json = ReadJson("DSMOOServer.Data.zones.json");
        _zones = JsonSerializer.Deserialize<List<string>>(json)!;
        
        base.Initialize();
    }

    private string ReadJson(string resourceName)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            logger.Error($"Stream of {resourceName} is null");
            return "[]";
        }
        using var reader = new StreamReader(stream!);
        var json = reader.ReadToEnd();
        stream.Close();
        reader.Close();
        return json;
    }
    
    public string GetConnection(string fromStage, string toStage, int index = 0)
    {
        var stage = Stages.FirstOrDefault(x => x.StageName == toStage);
        if (stage == null)
            return "";
        var warp = stage.Warps.Where(x => x.ConnectedStage == fromStage).ToArray();
        return warp.Length == 0 ? "" : warp[Math.Clamp(index, 0, warp.Length - 1)].Name;
    }

    public string? GetStageFromInput(string input)
    {
        foreach (var stageInfo in _stages)
        {
            if (string.Equals(input, stageInfo.StageName, StringComparison.InvariantCultureIgnoreCase))
                return stageInfo.StageName;

            foreach (var alias in stageInfo.Alias)
            {
                if (string.Equals(input, alias, StringComparison.InvariantCultureIgnoreCase))
                    return stageInfo.StageName;
            }
        }

        foreach (var zone in _zones)
        {
            if(input == zone)
                return zone;
        }
        
        return input.EndsWith('!') ? input.Substring(0, input.Length - 1) : null;
    }

    public string KingdomNames()
    {
        var kingdoms = new List<string>();
        foreach (var stage in _stages)
        {
            if (!stage.StageName.Contains("HomeStage") || stage.Alias.Length < 2)
                continue;
            kingdoms.Add($"{stage.Alias[0]}     ->  {stage.Alias[1]}");
        }
        return string.Join("\n", kingdoms);
    }
}