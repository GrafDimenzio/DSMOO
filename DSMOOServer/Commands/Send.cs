using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "send",
    Aliases = [],
    Description = "Sends a player to the specified stage",
    Parameters = ["stage", "warp-id", "scenario", "players"]
)]
public class Send(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 4)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: send [stage] [id] [scenario] [players]"
            };

        var stage = Stages.Input2Stage(args[0]);
        if (stage == null)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = "Invalid Stage Name\n" + Stages.KingdomAliasMapping()
            };

        if (Encoding.UTF8.GetBytes(args[1]).Length > ChangeStagePacket.IdSize)
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