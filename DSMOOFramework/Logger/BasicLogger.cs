namespace DSMOOFramework.Logger;

public abstract class BasicLogger : ILogger
{
    public delegate void LogHandler(string message, LogType type);

    protected BasicLogger()
    {
        Log += OnLogToGlobal;
    }

    public abstract string Name { get; set; }

    public void Info(object message)
    {
        Log?.Invoke(FormatMessage(message, LogType.Info), LogType.Info);
    }

    public void Error(object message)
    {
        Log?.Invoke(FormatMessage(message, LogType.Error), LogType.Error);
    }

    public void Error(object? message, Exception ex)
    {
        var msg = "";
        if (message != null)
            msg = message + "\n";

        Log?.Invoke(FormatMessage(msg + ex, LogType.Error), LogType.Error);
    }

    public void Warn(object message)
    {
        Log?.Invoke(FormatMessage(message, LogType.Warn), LogType.Warn);
    }

    public void Setup(object message)
    {
        Log?.Invoke(FormatMessage(message, LogType.Setup), LogType.Setup);
    }

    public void Debug(object message)
    {
        Log?.Invoke(FormatMessage(message, LogType.Debug), LogType.Debug);
    }

    public abstract ILogger Copy();

    private string FormatMessage(object message, LogType type)
    {
        return $"[{DateTime.Now.ToLocalTime()}] [{Name.ToUpper()}] [{type.ToString().ToUpper()}]: {message}";
    }

    private void OnLogToGlobal(string message, LogType type)
    {
        GlobalLog?.Invoke(message, type);
    }

    public event LogHandler? Log;
    public static event LogHandler? GlobalLog;
}