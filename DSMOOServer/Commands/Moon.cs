using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "moon",
    Aliases = [],
    Description = "Manages the moon sync",
    Parameters = ["[list/clear/sync/send/collect/active/include/exclude]"]
)]
public class Moon(MoonManager manager, PlayerManager playerManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult()
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: moon " + CommandInfo.Parameters[0],
            };
        }

        switch (args[0].ToLower())
        {
            case "list":
                var builder = new StringBuilder();
                builder.Append("Collected Shines:\n " + string.Join(", ", manager.MoonSync));
                if(manager.ExcludedMoons.Count > 0)
                    builder.Append("\nExcluded Moons:\n " + string.Join(", ", manager.ExcludedMoons));
                return builder.ToString();
            
            case "clear":
                manager.MoonSync.Clear();
                manager.SaveMoons();
                manager.SyncMoons();
                return "Cleared all Shines";
            
            case "sync":
                manager.SyncMoons();
                return "Synced Moons";
            
            case "send":
            case "collect":
                if (args.Length == 1 || !int.TryParse(args[1], out var id))
                    return new CommandResult()
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "Usage: moon send moon-id"
                    };
                if (args[0].ToLower() == "send")
                {
                    foreach (var player in playerManager.RealPlayers)
                    {
                        player.SendShine(id);
                    }
                    return $"Sent Moon {id} to all Players";
                }

                manager.AddMoon(id);
                return $"Added Moon {id} to the server's save file";
            
            case "include":
            case "exclude":
                if (args.Length == 1 || !int.TryParse(args[1], out id))
                    return new CommandResult()
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "Usage: moon [include/exclude] moon-id"
                    };
                if (args[0].Equals("include", StringComparison.CurrentCultureIgnoreCase))
                {
                    manager.ExcludedMoons.Remove(id);
                    manager.SaveMoons();
                    return $"Removed Moon {id} from the server's list of excluded Moons";
                }
                manager.ExcludedMoons.Add(id);
                manager.SaveMoons();
                return $"Added Moon {id} to the server's list of excluded Moons";
            
            default:
                return new CommandResult()
                {
                    ResultType = ResultType.InvalidParameter,
                    Message = "Usage: moon " + CommandInfo.Parameters[0],
                };
        }
    }
}