using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFlip;

public class TestManager(ILogger logger) : Manager
{
    public override void Initialize()
    {
        logger.Warn("Flip Plugin initialized");
    }
}