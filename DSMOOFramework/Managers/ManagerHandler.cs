using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Managers;

public class ManagerHandler(Analyzer.Analyzer analyzer, ObjectController objectController, ILogger logger)
{
    public ILogger Logger { get; } = logger;
    public Analyzer.Analyzer Analyzer { get; } = analyzer;
    public ObjectController ObjectController { get; } = objectController;

    public List<Manager> Managers { get; } = [];

    public void Setup()
    {
        Analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<Manager>()) return;
        if (args.Type == typeof(Manager)) return;
        if (args.Type.IsAbstract) return;
        if (ObjectController.GetObject(args.Type) is not Manager manager) return;
        Logger.Setup("Creating manager " + args.Type.Name);
        Managers.Add(manager);
        manager.Initialize();
    }
}