using Swan.Logging;
using ILogger = DSMOOFramework.Logger.ILogger;

namespace DSMOOWebInterface.Setup;

public class LogConverter(ILogger logger) : TextLogger, Swan.Logging.ILogger
{
    public void Dispose()
    {
    }

    public void Log(LogMessageReceivedEventArgs logEvent)
    {
        switch (logEvent.MessageType)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                logger.Debug(logEvent.Message);
                break;

            case LogLevel.Fatal:
            case LogLevel.Error:
                logger.Error(logEvent.Message);
                break;

            case LogLevel.Warning:
                logger.Warn(logEvent.Message);
                break;

            case LogLevel.Info:
                logger.Info(logEvent.Message);
                break;
        }
    }

    public LogLevel LogLevel => LogLevel.Trace;
}