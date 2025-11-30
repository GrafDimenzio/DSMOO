using DSMOOFramework.Logger;

namespace DSMOOConsole;

public class ConsoleLogger : BasicLogger
{
    public readonly LogType[] Logs = [LogType.Error, LogType.Warn, LogType.Info, LogType.Setup];
    
    public override string Name { get; set; } = "ConsoleLogger";

    public override ILogger Copy() => new ConsoleLogger() { Name = Name };

    public ConsoleLogger()
    {
        Log += OnLog;
    }

    private void OnLog(string message, LogType type)
    {
        if(!Logs.Contains(type)) return;
        
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