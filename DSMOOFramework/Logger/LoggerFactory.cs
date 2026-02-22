using DSMOOFramework.Controller;

namespace DSMOOFramework.Logger;

public class LoggerFactory<T>(T logger) : IFactory<T>
    where T : ILogger
{
    private readonly T _logger = logger;
    
    public T Create(Type? createFor)
    {
        var logger = _logger.Copy();
        if (createFor != null)
            logger.Name = createFor.Name;
        return (T)logger;
    }
}