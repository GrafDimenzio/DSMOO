using System.Reflection;
using DSMOOFramework.Events;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Analyzer;

public class Analyzer(ILogger logger)
{
    public EventReactor<AnalyzeEventArgs> OnAnalyze { get; } = new(logger);

    private ILogger Logger { get; } = logger;

    public void AnalyzeAssemblies(Assembly[] assemblies)
    {
        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            Logger.Setup($"Preparing for analyzing Assembly {assembly.GetName().Name}");
            types.AddRange(assembly.GetTypes());
        }

        AnalyzeTypes(types.ToArray());
    }

    public void AnalyzeAssembly(Assembly assembly)
    {
        Logger.Setup("Checking assembly " + assembly.GetName().Name);
        AnalyzeTypes(assembly.GetTypes());
    }

    public void AnalyzeTypes(Type[] types)
    {
        var eventArgs = new List<AnalyzeEventArgs>();

        foreach (var type in types)
        {
            var args = new AnalyzeEventArgs(type);
            eventArgs.Add(args);
        }

        foreach (var arg in eventArgs.OrderByDescending(x => x.Priority))
        {
            OnAnalyze.RaiseEvent(arg);
        }
    }
}