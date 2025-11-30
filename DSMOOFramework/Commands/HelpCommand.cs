namespace DSMOOFramework.Commands;

[Command(
    CommandName = "help",
    Aliases = [],
    Parameters = [],
    Description = "Command that displays a list of available commands."
    )]
public class HelpCommand(CommandManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        var msg = "All Available Commands:";
        foreach (var cmd in manager.Commands)
        {
            msg +=
                $"\n\n{cmd.CommandInfo.CommandName} - {cmd.CommandInfo.Description}\n  Alias: {string.Join(", ", cmd.CommandInfo.Aliases)}\n  Parameters: {string.Join(", ", cmd.CommandInfo.Parameters)}";
        }

        return msg;
    }
}