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
public class FlipCommand(PlayerManager manager, FlipManager flip) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length == 0)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "You need to specify a sub command [list/add/remove/set/pov]"
            };

        switch (args[0].ToLower())
        {
            case "list":
                return $"Flip POV: {flip.FlipOptions}\n" + "User ids:\n - " + string.Join("\n   - ", flip.Players);

            case "remove":
            case "add":
                if (args.Length == 1)
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message = "Usage: flip add <* | !* (usernames to not flip...) | (usernames to flip...)>"
                    };
                var add = args[0].ToLower() == "add";
                var players = manager.SearchForPlayers(args[1..]);
                foreach (var player in players.Players)
                {
                    if (add)
                        flip.Players.Add(player.Id);
                    else
                        flip.Players.Remove(player.Id);

                    flip.SaveConfig();
                }

                return MessageHelper.FormatMessage(players, add ? "flipped" : "removed flip");

            case "set":
                if (args.Length <= 2 || !bool.TryParse(args[1], out add))
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message =
                            "Usage: flip set [true/false] <* | !* (usernames to not flip...) | (usernames to flip...)>"
                    };

                players = manager.SearchForPlayers(args[2..]);
                foreach (var player in players.Players)
                {
                    if (add)
                        flip.Players.Add(player.Id);
                    else
                        flip.Players.Remove(player.Id);

                    flip.SaveConfig();
                }

                return MessageHelper.FormatMessage(players, $"Set flipped to {add} for");

            case "pov":
                if (args.Length == 1 || !Enum.TryParse<FlipOptions>(args[1], true, out var options))
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message = "Usage: flip pov [self/other/both]"
                    };
                flip.FlipOptions = options;

                return $"Set Flip POV to {options}";

            default:
                return new CommandResult
                {
                    ResultType = ResultType.InvalidParameter,
                    Message = "You need to specify a sub command [list/add/remove/set/pov]"
                };
        }
    }
}