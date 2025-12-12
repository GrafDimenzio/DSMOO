using DSMOOFramework.Config;

namespace DSMOOFlip;

[Config(Name = "flip")]
public class FlipConfig : IConfig
{
    public bool Enabled { get; set; } = true;
    public FlipOptions Options { get; set; } = FlipOptions.Other | FlipOptions.Self;
    public HashSet<Guid> Players { get; set; } = [];
}