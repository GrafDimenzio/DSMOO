using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.API.Stage;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "sendall",
    Aliases = [],
    Description = "Sends all players to the stage",
    Parameters = ["stage", "(warp-id)"]
)]
public class SendAll(PlayerManager manager, StageManager stageManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 1)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: sendall [stage] optional warp id"
            };

        var stage = stageManager.GetStageFromInput(args[0]);
        if (stage == null)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = $"Invalid Stage Name: {args[0]}\n" + stageManager.KingdomNames()
            };

        var warpId = "";

        if (args.Length > 1)
        {
            warpId = args[1];
            if (Encoding.UTF8.GetBytes(args[1]).Length > Constants.WarpIdSize)
                return new CommandResult
                {
                    ResultType = ResultType.InvalidParameter,
                    Message = "Warp-Id is too long"
                };
        }

        foreach (var player in manager.Players)
            player.ChangeStage(stage, warpId);

        return $"Send all Players to {stage}{(warpId != "" ? $"-{warpId}" : "")}";
    }
}