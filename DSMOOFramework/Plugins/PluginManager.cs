using System.Collections.ObjectModel;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Plugins;

[Analyze(Priority = 1)]
public class PluginManager : Manager
{
    private readonly List<PluginAttribute> plugins = [];
    private readonly ILogger _logger;
    private readonly AssemblyManager _assemblyManager;
    private readonly Analyzer.Analyzer _analyzer;
    
    public string PluginDirectory { get; }
    
    public ReadOnlyCollection<PluginAttribute> Plugins => plugins.AsReadOnly();
    
    public PluginManager(ILogger logger, PathLocation pathLocation, AssemblyManager assemblyManager, Analyzer.Analyzer analyzer)
    {
        _logger = logger;
        _assemblyManager = assemblyManager;
        _analyzer = analyzer;
        PluginDirectory = pathLocation.GetPath("plugins") ?? "";
        if (!Directory.Exists(PluginDirectory))
            Directory.CreateDirectory(PluginDirectory);
    }
    
    public void LoadPlugins()
    {
        _logger.Setup("Loading Dll's from Directory: " + PluginDirectory);
        _assemblyManager.LoadAssemblies(PluginDirectory);
    }

    public override void Initialize()
    {
        _analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        var attribute = args.GetAttribute<PluginAttribute>();
        if (attribute == null) return;
        plugins.Add(attribute);
    }
}