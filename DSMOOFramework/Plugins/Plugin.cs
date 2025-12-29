using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Plugins;

public abstract class Plugin<T> : Manager, IInject where T : IConfig
{
    public T Config => (T)ConfigHolder.ConfigObject;

    [Inject] public ConfigHolder<T> ConfigHolder { get; set; }

    [Inject] public ILogger Logger { get; set; }

    public virtual void AfterInject()
    {
    }

    public void SaveConfig()
    {
        ConfigHolder.SaveConfig();
    }
}