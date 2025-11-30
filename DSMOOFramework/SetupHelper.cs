using System.Reflection;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Commands;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework;

public static class SetupHelper
{
    public static Controller.ObjectController BasicSetup(ILogger controllerLogger, Assembly? assembly = null,
        ILogger? analyzerLogger = null, ILogger? managerLogger = null)
    {
        var controller = new Controller.ObjectController(controllerLogger);
        var analyzer = controller.GetObject<Analyzer.Analyzer>(analyzerLogger ?? controllerLogger);
        var managerHandler = controller.GetObject<ManagerHandler>(managerLogger ?? controllerLogger);
        if (analyzer == null || managerHandler == null)
        {
            controllerLogger.Error("Could not create Analyzer or ManagerHandler");
            throw new Exception("Analyzer or ManagerHandler not created");
        }
        managerHandler.Setup();
        analyzer.AnalyzeAssembly(Assembly.GetExecutingAssembly());
        if (assembly != null)
        {
            analyzer.AnalyzeAssembly(assembly);
        }
        
        return controller;
    }
}