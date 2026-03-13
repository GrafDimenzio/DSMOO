using DSMOOFramework.Analyzer;
using DSMOOFramework.Commands;

namespace DSMOOFramework.Plugins;

[Analyze(Priority = -1)]
[Command(
    CommandName = "plugin",
    Aliases = ["plugins"],
    Parameters = [],
    Description = "Command that displays a list of all plugins."
)]
public class PluginCommand(PluginManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        var msg = "All Plugins:";
        foreach (var plugin in manager.Plugins)
            msg +=
                $"\n\n{plugin.Name} - {plugin.Description}\n  Version: {plugin.Version}\n  Author: {plugin.Author}\n  Repository: {plugin.Repository}";

        return msg;
    }
}