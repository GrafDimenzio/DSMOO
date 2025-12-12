using System.Reflection;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;

namespace DSMOOFramework;

public static class SetupHelper
{
    public static ObjectController BasicSetup(ILogger controllerLogger, Dictionary<string, string> paths,
        Assembly[]? assemblies = null,
        ILogger? analyzerLogger = null, ILogger? managerLogger = null)
    {
        var controller = new ObjectController(controllerLogger);
        var pathLocation = controller.GetObject<PathLocation>();
        if (pathLocation == null)
        {
            controllerLogger.Error("Could not create PathLocation");
            throw new Exception("PathLocation not created");
        }

        pathLocation.AddPaths(paths);
        var analyzer = controller.GetObject<Analyzer.Analyzer>(analyzerLogger ?? controllerLogger);
        var managerHandler = controller.GetObject<ManagerHandler>(managerLogger ?? controllerLogger);
        if (analyzer == null || managerHandler == null)
        {
            controllerLogger.Error("Could not create Analyzer or ManagerHandler");
            throw new Exception("Analyzer or ManagerHandler not created");
        }

        managerHandler.Setup();
        analyzer.AnalyzeAssembly(Assembly.GetExecutingAssembly());
        if (assemblies != null) analyzer.AnalyzeAssemblies(assemblies);

        controller.GetObject<PluginManager>()!.LoadPlugins();

        return controller;
    }
}