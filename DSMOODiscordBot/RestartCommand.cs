using DSMOOFramework.Commands;

namespace DSMOODiscordBot;

[Command(
    CommandName = "dcrestart",
    Aliases = ["discordrestart"],
    Parameters = [],
    Description = "Restarts the Discord Bot"
    )]
public class RestartCommand(DiscordBot bot) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        Task.Run(bot.Reconnect);
        return "Restarting the Discord Bot";
    }
}