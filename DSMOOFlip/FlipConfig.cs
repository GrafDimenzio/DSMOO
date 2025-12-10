using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;

namespace DSMOOFlip;

[Analyze(Priority = 2)]
[Config(Name = "flip")]
public class FlipConfig : IConfig
{
    public bool Enabled { get; set; } = true;
    public FlipOptions Options { get; set; } = FlipOptions.Other | FlipOptions.Self;
    public HashSet<Guid> Players { get; set; } = [];
}