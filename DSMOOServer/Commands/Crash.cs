using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "crash",
    Aliases = [],
    Description = "Crashes a player",
    Parameters = ["players"]
)]
public class Crash(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult()
            {
                ResultType = ResultType.MissingParameter,
                Message = "You need to specify a player",
            };
        }
        
        var players = manager.SearchForPlayers(args);
        foreach (var player in players.Players)
        {
            player.Crash(false);
        }

        return MessageHelper.FormatMessage(players, "Crashed");
    }
}