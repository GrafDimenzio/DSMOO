using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;

namespace DSMOOServer;

[Analyze(Priority = 10000)]
[Config(Name = "settings")]
public class ServerMainConfig : IConfig
{
    public string Address { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 1027;
    public bool MoonSyncEnabled { get; set; } = true;
    public ISet<int> ExcludedMoons { get; set; } = new SortedSet<int> { 496 };
    public bool ClearOnNewSaves {get; set;} = true;
    public bool ScenarioMerging { get; set; } = true;
    public ushort MaxPlayers { get; set; } = 4;
}