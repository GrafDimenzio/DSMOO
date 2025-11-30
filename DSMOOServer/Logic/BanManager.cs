using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Managers;

namespace DSMOOServer.Logic;

[Analyze(Priority = 1)]
public class BanManager(ConfigHolder<BanList> banList) : Manager
{
    
}

[Analyze(Priority = 2)]
[Config(Name = "banlist")]
public class BanList : IConfig
{
    public bool Enabled { get; set; } = true;
    public string[] IPs { get; set; } = [];
    public Guid[] Profiles { get; set; } = [];
    public string[] Stages { get; set; } = [];
    public sbyte[] GameModes { get; set; } = [];
}