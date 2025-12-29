using System.Collections.ObjectModel;
using System.Reflection;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Managers;

public class AssemblyManager : Manager
{
    private readonly Analyzer.Analyzer _analyzer;

    private readonly List<Assembly> _assemblies = [];

    public AssemblyManager(ILogger logger, Analyzer.Analyzer analyzer)
    {
        _analyzer = analyzer;
        Logger = logger;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    public ILogger Logger { get; }
    public ReadOnlyCollection<Assembly> Assemblies => _assemblies.AsReadOnly();

    private Assembly ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        return _assemblies.FirstOrDefault(x => x.FullName == args.Name)!;
    }

    public Assembly LoadAssembly(string path)
    {
        var assembly = Assembly.LoadFrom(path);
        _assemblies.Add(assembly);
        _analyzer.AnalyzeAssembly(assembly);
        return assembly;
    }

    public Assembly[] LoadAssemblies(string directory)
    {
        return LoadAssemblies(Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories));
    }

    public Assembly[] LoadAssemblies(string[] paths)
    {
        var assemblies = new List<Assembly>();
        foreach (var path in paths)
            try
            {
                var assembly = Assembly.LoadFrom(path);
                assemblies.Add(assembly);
                Logger.Setup($"Loaded Assembly: {assembly.GetName().Name}");
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading a Assembly", e);
            }

        _analyzer.AnalyzeAssemblies(assemblies.ToArray());
        return assemblies.ToArray();
    }
}