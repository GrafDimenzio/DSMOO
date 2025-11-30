using DSMOOFramework.Analyzer;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;

namespace DSMOOFramework.Commands;

[Analyze(Priority = 1)]
public class CommandManager(Analyzer.Analyzer analyzer, Controller.ObjectController objectController, ILogger logger) : Manager
{
    public List<ICommand> Commands { get; set; } = [];
    
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }
    
    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<ICommand>()) return;
        var info = args.GetAttribute<CommandAttribute>();
        if (info == null) return;
        if (objectController.GetObject(args.Type) is not ICommand command) return;
        AddCommand(command, info);
    }

    public void AddCommand(ICommand command, CommandAttribute info)
    {
        command.CommandInfo = info;
        Commands.Add(command);
        logger.Setup($"Added command {command.CommandInfo.CommandName}");
    }

    public CommandResult ProcessQuery(string query)
    {
        var split = query.Trim(' ').Split(' ');
        var cmd = GetCommand(split[0]);
        if (cmd == null)
        {
            return new CommandResult()
            {
                ResultType = ResultType.NotFound,
                Message = "Invalid command. Use help for a list of commands"
            };
        }

        try
        {
            var preResult = cmd.PreExecute(split[0], split[1..]);
            return preResult.ResultType != ResultType.Success ? preResult : cmd.Execute(split[0], split[1..]);
        }
        catch (Exception e)
        {
            return new CommandResult
            {
                ResultType = ResultType.Error,
                Message = e.Message + "\n" + e.ToString()
            };
        }
    }

    private ICommand? GetCommand(string command)
    {
        foreach (var iCommand in Commands)
        {
            if (string.Equals(iCommand.CommandInfo.CommandName, command, StringComparison.OrdinalIgnoreCase))
                return iCommand;
            foreach (var alias in iCommand.CommandInfo.Aliases)
            {
                if (string.Equals(alias, command, StringComparison.OrdinalIgnoreCase))
                    return iCommand;
            }
        }
        return null;
    }
}