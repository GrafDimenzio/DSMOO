namespace DSMOOFramework.Config;

public class ConfigHolder<TConfig>(ConfigManager configManager) : IConfigHolder where TConfig : IConfig
{
    
    public TConfig Config => (TConfig)ConfigObject!;

    public IConfig ConfigObject { get; set; }
    
    public void SaveConfig() => configManager.SaveConfig(ConfigObject);
}