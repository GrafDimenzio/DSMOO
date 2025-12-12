using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Plugins;

public abstract class Plugin<T> : Manager where T : IConfig
{
    public T Config => (T)ConfigHolder.ConfigObject;
    
    [Inject]
    public ConfigHolder<T> ConfigHolder { get; set; }
    public void SaveConfig() => ConfigHolder.SaveConfig();
}