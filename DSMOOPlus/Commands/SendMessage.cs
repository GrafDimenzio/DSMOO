using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOPlus.Commands;

[Command(
    CommandName = "sendmessage",
    Aliases = ["message", "msg"],
    Description = "Sends a message to the specified client",
    Parameters = ["message", "players"]
)]
public class SendMessage(PlayerManager playerManager) : Command
{
    public override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        if (args.Length <= 1)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "You need to specify a message and players. Example usage: msg \"my awesome message\" *"
            };

        var playerArgs = args[1..];
        var players = playerManager.SearchForPlayers(playerArgs.Length == 1 ? playerArgs[0].Split(' ') : playerArgs);

        foreach (var player in players.Players) player.GetComponent<PlayerPlus>()!.SendMessage(args[0]);

        return MessageHelper.FormatMessage(players, "send message to");
    }
}