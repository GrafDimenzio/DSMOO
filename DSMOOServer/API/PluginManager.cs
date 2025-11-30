using DSMOOFramework.Analyzer;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOServer.Logic;

[Analyze(Priority = -100)]
public class PluginManager(ILogger logger, AssemblyManager assemblyManager) : Manager
{
    public override void Initialize()
    {
        var pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (!Directory.Exists(pluginPath))
            Directory.CreateDirectory(pluginPath);
        assemblyManager.LoadAssemblies(pluginPath);
    }
}