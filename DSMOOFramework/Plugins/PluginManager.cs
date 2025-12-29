using System.Collections.ObjectModel;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Events;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Plugins;

public class PluginManager : Manager
{
    private readonly Analyzer.Analyzer _analyzer;
    private readonly AssemblyManager _assemblyManager;
    private readonly ILogger _logger;
    private readonly List<PluginAttribute> plugins = [];

    public readonly EventReactor<EventArg> OnPluginLoaded;

    public PluginManager(ILogger logger, PathLocation pathLocation, AssemblyManager assemblyManager,
        Analyzer.Analyzer analyzer)
    {
        _logger = logger;
        OnPluginLoaded = new EventReactor<EventArg>(logger);
        _assemblyManager = assemblyManager;
        _analyzer = analyzer;
        PluginDirectory = pathLocation.GetPath("plugins") ?? "";
        if (!Directory.Exists(PluginDirectory))
            Directory.CreateDirectory(PluginDirectory);
    }

    public string PluginDirectory { get; }

    public ReadOnlyCollection<PluginAttribute> Plugins => plugins.AsReadOnly();

    public void LoadPlugins()
    {
        _logger.Info("Loading plugins...");
        _logger.Setup("Loading Dll's from Directory: " + PluginDirectory);
        _assemblyManager.LoadAssemblies(PluginDirectory);
        OnPluginLoaded.RaiseEvent(new());
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