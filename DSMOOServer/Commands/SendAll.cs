using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "sendall",
    Aliases = [],
    Description = "Sends all players to the stage",
    Parameters = ["stage", "(warp-id)"]
)]
public class SendAll(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 1)
        {
            return new CommandResult()
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: sendall [stage] optional warp id",
            };
        }
        
        var stage = Stages.Input2Stage(args[0]);
        if (stage == null)
        {
            return new CommandResult()
            {
                ResultType = ResultType.InvalidParameter,
                Message = "Invalid Stage Name\n" + Stages.KingdomAliasMapping(),
            };
        }

        var warpId = "";

        if (args.Length > 1)
        {
            warpId = args[1];
            if (Encoding.UTF8.GetBytes(args[1]).Length > 0x10)
                return new CommandResult()
                {
                    ResultType = ResultType.InvalidParameter,
                    Message = "Warp-Id is too long",
                };
        }
        
        foreach (var player in manager.Players)
        {
            player.ChangeStage(stage,warpId, -1, 0);
        }

        return $"Send all Players to {stage}{(warpId != "" ? ($"-{warpId}") : (""))}";
    }
}