using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using DSMOOFramework;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Stage.Map;

namespace DSMOOServer.API.Stage;

public class StageManager(ILogger logger, PathLocation pathLocation) : Manager
{
    private List<MapInfo> _maps = [];
    private List<StageInfo> _stages = [];
    private List<string> _zones = [];

    public ReadOnlyCollection<StageInfo> Stages => new(_stages);
    public ReadOnlyCollection<string> Zones => new(_zones);
    public ReadOnlyCollection<MapInfo> MapInfo => new(_maps);

    public override void Initialize()
    {
        InitialiseVanillaLogic();
        ReadMods();

        base.Initialize();
    }

    private void InitialiseVanillaLogic()
    {
        var json = ReadResource("DSMOOServer.Data.stages.json");
        _stages = JsonSerializer.Deserialize<List<StageInfo>>(json)!;

        json = ReadResource("DSMOOServer.Data.zones.json");
        _zones = JsonSerializer.Deserialize<List<string>>(json)!;

        json = ReadResource("DSMOOServer.Data.map_data.json");
        _maps = JsonSerializer.Deserialize<List<MapInfo>>(json)!;
    }

    private string ReadResource(string resourceName)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            logger.Error($"Stream of {resourceName} is null");
            return "[]";
        }

        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();
        stream.Close();
        reader.Close();
        return result;
    }

    private void ReadMods()
    {
        var path = pathLocation.GetPath("mods");
        if (path == null)
        {
            logger.Error("No Mod Path was specified");
            return;
        }

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var content = File.ReadAllText(file);
            switch (name)
            {
                case var s when s.EndsWith(".map_data"):
                    var maps = JsonSerializer.Deserialize<List<MapInfo>>(content) ?? [];
                    _maps.AddRange(maps);
                    break;

                case var s when s.EndsWith(".stages"):
                    var stages = JsonSerializer.Deserialize<List<StageInfo>>(content) ?? [];
                    _stages.AddRange(stages);
                    break;

                case var s when s.EndsWith(".zones"):
                    var zones = JsonSerializer.Deserialize<List<string>>(content) ?? [];
                    _zones.AddRange(zones);
                    break;
            }
        }
    }

    public string? GetKingdomFromStage(string stage)
    {
        foreach (var kingdom in _maps)
        {
            if (stage == kingdom.MainStageName)
                return kingdom.MainStageName;

            foreach (var subArea in kingdom.SubAreas)
                if (subArea.SubAreaName == stage)
                    return kingdom.MainStageName;
        }

        return null;
    }

    public MapLocation? GetMapLocation(Vector3 position, string stageName)
    {
        var kingdom = GetKingdomFromStage(stageName);
        if (kingdom == null)
            return null;

        var infoTable = _maps.FirstOrDefault(x => x.MainStageName == kingdom);
        if (infoTable == null)
            return null;

        var location = new MapLocation
        {
            Kingdom = kingdom
        };

        if (stageName == kingdom)
        {
            location.X = position.X;
            location.Y = position.Z;
        }
        else if (infoTable.SubAreas.Any(x => x.SubAreaName == stageName))
        {
            var sub = infoTable.SubAreas.First(x => x.SubAreaName == stageName);
            location.X = sub.Position.X;
            location.Y = sub.Position.Y;
            location.SubArea = true;
        }
        else
        {
            location.X = 2048;
            location.Y = 2048;
        }

        if (infoTable.Rotation != 0)
        {
            var angle = Math.PI * -infoTable.Rotation;
            var oldX = location.X;
            var oldY = location.Y;

            location.Y = oldY * Math.Cos(angle) - oldX * Math.Sin(angle);
            location.X = oldY * Math.Sin(angle) + oldX * Math.Cos(angle);
        }

        location.X += infoTable.Offset.X;
        location.Y += infoTable.Offset.Y;

        location.X /= infoTable.Scale;
        location.Y /= infoTable.Scale;

        location.Number = (int)((location.X - 65) / 390) + 1;
        location.Letter = (int)((location.Y - 65) / 390) + 1;
        return location;
    }

    public string GetConnection(string fromStage, string toStage, int index = 0)
    {
        var stage = Stages.FirstOrDefault(x => x.StageName == toStage);
        if (stage == null)
            return "";
        var warp = stage.Warps.Where(x => x.ConnectedStage == fromStage).ToArray();
        return warp.Length == 0 ? "" : warp[Math.Clamp(index, 0, warp.Length - 1)].Name;
    }

    public StageInfo? GetStageInfo(string stage)
    {
        foreach (var stageInfo in _stages)
            if (stageInfo.StageName == stage)
                return stageInfo;
        return null;
    }

    public string? GetNearestWarp(string stage, Vector3 position)
    {
        var stageInfo = GetStageInfo(stage);
        if (stageInfo == null)
            return null;
        var lowestDistance = float.MaxValue;
        var warpName = "";
        foreach (var warp in stageInfo.Warps)
        {
            if (warp.Position == Vector3.Zero)
                continue;

            var distance = (position - warp.Position).LengthSquared();
            if (distance >= lowestDistance)
                continue;
            lowestDistance = distance;
            warpName = warp.Name;
        }

        return warpName;
    }

    public string? GetStageFromInput(string input)
    {
        foreach (var stageInfo in _stages)
        {
            if (string.Equals(input, stageInfo.StageName, StringComparison.InvariantCultureIgnoreCase))
                return stageInfo.StageName;

            foreach (var alias in stageInfo.Alias)
                if (string.Equals(input, alias, StringComparison.InvariantCultureIgnoreCase))
                    return stageInfo.StageName;
        }

        foreach (var zone in _zones)
            if (input == zone)
                return zone;

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