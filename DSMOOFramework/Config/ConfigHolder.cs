namespace DSMOOFramework.Config;

public class ConfigHolder<TConfig> : IConfigHolder where TConfig : IConfig
{
    public TConfig Config => (TConfig)ConfigObject!;

    public IConfig ConfigObject { get; set; }
}