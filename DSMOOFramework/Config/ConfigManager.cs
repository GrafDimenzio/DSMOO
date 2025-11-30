using System.Reflection;
using System.Text.Json;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Config;

public class ConfigManager(ILogger logger, Analyzer.Analyzer analyzer, ObjectController controller) : Manager
{
    private Type _generichConfigHolderType = typeof(ConfigHolder<>);
    
    public Dictionary<string,IConfigHolder> Configs { get; set; } = new();
    
    public string ConfigPath { get; private set; }

    public IConfig? LoadConfig(Type type)
    {
        var name = GetConfigName(type);
        var json = ReadConfig(type);
        var jsonObject = JsonSerializer.Deserialize(json, type);
        if (jsonObject is not IConfig config)
            return null;
        
        var holderType = _generichConfigHolderType.MakeGenericType(type);
        var holder = (IConfigHolder)controller.GetObject(holderType)!;
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
        var path = name + ".json";
        if(!File.Exists(path))
            File.Create(path).Close();
        File.WriteAllText(path, json);
        Configs[name].ConfigObject = config;
    }
    
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private string ReadConfig(Type type)
    {
        var name = GetConfigName(type);
        var path = name + ".json";
        if (File.Exists(path)) return File.ReadAllText(path);
        if (controller.GetObject(type) is not IConfig config)
        {
            return "{}";
        }
        SaveConfig(config);
        return File.ReadAllText(path);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<IConfig>()) return;
        if(!args.HasAttribute<ConfigAttribute>()) return;
        LoadConfig(args.Type);
    }

    private string GetConfigName(Type type)
    {
        var attribute = type.GetCustomAttribute<ConfigAttribute>();
        if (attribute == null) return "";
        return attribute.Name;
    }
}