using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.API.Stage;
using DSMOOServer.Helper;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "send",
    Aliases = [],
    Description = "Sends a player to the specified stage",
    Parameters = ["stage", "warp-id", "scenario", "players"]
)]
public class Send(PlayerManager manager, StageManager stageManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 4)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: send [stage] [id] [scenario] [players]"
            };

        var stage = stageManager.GetStageFromInput(args[0]);
        if (stage == null)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = "Invalid Stage Name\n" + stageManager.KingdomNames()
            };

        if (Encoding.UTF8.GetBytes(args[1]).Length > Constants.WarpIdSize)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = "Warp-Id is too long"
            };

        if (!sbyte.TryParse(args[2], out var scenario) || scenario < -1)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = "Invalid Scenario number (range: [-1 to 127])"
            };

        var players = manager.SearchForPlayers(args[3..]);
        foreach (var player in players.Players)
            player.ChangeStage(stage, args[1], scenario);

        return MessageHelper.FormatMessage(players, "Send");
    }
}