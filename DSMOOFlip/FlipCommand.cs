using DSMOOFramework.Commands;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOFlip;

[Command(
    CommandName = "flip",
    Aliases = [],
    Description = "flips a player",
    Parameters = ["[list/add/remove/set/pov]"]
)]
public class FlipCommand(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult()
            {
                ResultType = ResultType.MissingParameter,
                Message = "You need to specify a sub command [list/add/remove/set/pov]",
            };
        }

        return "flip";
    }
}