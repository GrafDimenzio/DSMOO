using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;

namespace DSMOOFlip;

[Plugin(
    Name = "Flip",
    Description = "Plugin that adds the Flip Command",
    Author = "Dimenzio",
    Version = "1.0.0",
    Repository = "https://github.com/GrafDimenzio/DSMOO"
    )]
public class Init(ILogger logger) : Manager
{
    public override void Initialize()
    {
        logger.Warn("Flip Plugin initialized");
    }
}