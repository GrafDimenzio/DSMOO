using DSMOOFramework.Logger;

namespace DSMOOConsole;

public class ConsoleLogger : BasicLogger
{
    public readonly LogType[] Logs;

    public ConsoleLogger(LogType[] logs)
    {
        Logs = logs;
        Log += OnLog;
    }

    public override string Name { get; set; } = "ConsoleLogger";

    public override ILogger Copy()
    {
        return new ConsoleLogger(Logs) { Name = Name };
    }

    private void OnLog(string message, LogType type)
    {
        if (!Logs.Contains(type)) return;

        switch (type)
        {
            case LogType.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;

            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case LogType.Warn:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case LogType.Setup:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;

            case LogType.Debug:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
        }

        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}