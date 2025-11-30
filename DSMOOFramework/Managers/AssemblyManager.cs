using System.Collections.ObjectModel;
using System.Reflection;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Managers;

public class AssemblyManager(ILogger logger, Analyzer.Analyzer analyzer) : Manager
{
    public ILogger Logger { get; set; } = logger;
    
    private readonly List<Assembly> _assemblies = [];
    public ReadOnlyCollection<Assembly> Assemblies => _assemblies.AsReadOnly();

    public Assembly LoadAssembly(string path)
    {
        return LoadAssembly(File.ReadAllBytes(path));
    }
    
    public Assembly LoadAssembly(byte[] bytes)
    {
        var assembly = AppDomain.CurrentDomain.Load(bytes);
        _assemblies.Add(assembly);
        analyzer.AnalyzeAssembly(assembly);
        return assembly;
    }

    public Assembly[] LoadAssemblies(string directory)
        => LoadAssemblies(Directory.GetFiles(directory));

    public Assembly[] LoadAssemblies(string[] paths)
    {
        var assemblies = new List<Assembly>();
        foreach (var path in paths)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var assembly = AppDomain.CurrentDomain.Load(bytes);
                assemblies.Add(assembly);
                Logger.Setup($"Loaded Assembly: {assembly.GetName().Name}");
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading a Assembly", e);
            }
        }
        analyzer.AnalyzeAssemblies(assemblies.ToArray());
        return assemblies.ToArray();
    }
}