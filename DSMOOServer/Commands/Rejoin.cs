using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "rejoin",
    Aliases = [],
    Description = "Disconnects a player and forces him to rejoin the server.",
    Parameters = ["players"]
)]
public class Rejoin(PlayerManager manager) : Command
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
            player.Disconnect();
        }

        return MessageHelper.FormatMessage(players, "Rejoined");
    }
}