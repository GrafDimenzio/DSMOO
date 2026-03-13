using DSMOOFramework.Commands;
using DSMOOPlus.Packets;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOPlus.Commands;

[Command(
    CommandName = "sethealth",
    Aliases = ["health"],
    Description = "Sets the health of the players",
    Parameters = ["players", "health", "assistMode"]
)]
public class SetHealth(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        if (args.Length < 2)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "usage sethealth (player) (health) (optional true/false for assist health)"
            };
        if (!byte.TryParse(args[1], out var health))
            return new CommandResult()
            {
                ResultType = ResultType.InvalidParameter,
                Message = "invalid number for health"
            };
        
        if(args.Length == 2 || !bool.TryParse(args[2], out var isAssist))
            isAssist = false;

        var players = manager.SearchForPlayers(args[0].Split(' '));
        foreach (var player in players.Players)
            player.GetComponent<PlayerPlus>().SetHealth(health, isAssist);

        return MessageHelper.FormatMessage(players, $"Set Health to {health} for ");
    }
}