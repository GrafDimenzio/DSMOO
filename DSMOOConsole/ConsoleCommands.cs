using DSMOOFramework.Commands;

namespace DSMOOConsole;

public class ConsoleCommands(CommandManager manager)
{
    public void ListenForCommands()
    {
        ReadLine.HistoryEnabled = true;
        while (true)
        {
            var line = ReadLine.Read();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var response = manager.ProcessQuery(line);
            Console.ForegroundColor = response.ResultType switch
            {
                ResultType.Success => ConsoleColor.White,
                ResultType.Error => ConsoleColor.Red,
                ResultType.MissingParameter => ConsoleColor.DarkYellow,
                ResultType.InvalidParameter => ConsoleColor.DarkRed,
                ResultType.NoPermission => ConsoleColor.DarkCyan,
                ResultType.NotFound => ConsoleColor.DarkMagenta,
                _ => throw new ArgumentOutOfRangeException()
            };

            Console.WriteLine($"[{response.ResultType}] " + response.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}