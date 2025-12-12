using DSMOOFramework.Config;

namespace DSMOOServer;

[Config(Name = "settings")]
public class ServerMainConfig : IConfig
{
    public string Address { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 1027;
    public bool MoonSyncEnabled { get; set; } = true;
    public SortedSet<int> ExcludedMoons { get; set; } = [496];
    public bool ClearOnNewSaves { get; set; } = true;
    public bool ScenarioMerging { get; set; } = true;
    public ushort MaxPlayers { get; set; } = 4;
}