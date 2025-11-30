using System.Reflection;
using System.Text.Json;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Config;

public class ConfigManager : Manager
{
    private readonly ObjectController _controller;
    private readonly Analyzer.Analyzer _analyzer;
    
    public ConfigManager(Analyzer.Analyzer analyzer, ObjectController controller, PathLocation pathLocation)
    {
        ConfigPath = pathLocation.GetPath("config") ?? "";
        if(!Directory.Exists(ConfigPath))
            Directory.CreateDirectory(ConfigPath);
        _controller = controller;
        _analyzer = analyzer;
    }
    
    private readonly Type _genericConfigHolderType = typeof(ConfigHolder<>);
    
    public Dictionary<string,IConfigHolder> Configs { get; set; } = new();

    public string ConfigPath { get; }

    public IConfig? LoadConfig(Type type)
    {
        var name = GetConfigName(type);
        var json = ReadConfig(type);
        var jsonObject = JsonSerializer.Deserialize(json, type);
        if (jsonObject is not IConfig config)
            return null;
        
        var holderType = _genericConfigHolderType.MakeGenericType(type);
        var holder = (IConfigHolder)_controller.GetObject(holderType)!;
        holder.ConfigObject = config;
        
        Configs[name] = holder;
        //This is for auto repair reason. For Example if a update added a new field it will now be created after loading all old values
        SaveConfig(config);
        return config;
    }

    public void SaveConfig(IConfig config)
    {
        var name = GetConfigName(config.GetType());
        var json = JsonSerializer.Serialize(config, config.GetType(),
            new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(ConfigPath, name + ".json");
        if(!File.Exists(path))
            File.Create(path).Close();
        File.WriteAllText(path, json);
        if(Configs.TryGetValue(name, out var holder))
            holder.ConfigObject = config;
    }

    private string ReadConfig(Type type)
    {
        var name = GetConfigName(type);
        var path = Path.Combine(ConfigPath, name + ".json");
        if (File.Exists(path)) return File.ReadAllText(path);
        if (_controller.GetObject(type) is not IConfig config)
        {
            return "{}";
        }
        SaveConfig(config);
        return File.ReadAllText(path);
    }
    
    private string GetConfigName(Type type)
    {
        var attribute = type.GetCustomAttribute<ConfigAttribute>();
        return attribute?.Name ?? "";
    }
    
    public override void Initialize()
    {
        _analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<IConfig>()) return;
        if (!args.HasAttribute<ConfigAttribute>()) return;
        LoadConfig(args.Type);
    }
}