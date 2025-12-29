using DSMOOFramework.Commands;
using DSMOOFramework.Config;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "config",
    Aliases = [],
    Description = "Set some configs through the console",
    Parameters = ["[merge/maxplayer/load]", "(value)"]
)]
public class Config(
    ConfigHolder<ServerMainConfig> configHolder,
    ConfigManager configManager,
    PlayerManager playerManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 1)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: config [merge/maxplayer]"
            };

        switch (args[0])
        {
            case "load":
                foreach (var config in configManager.Configs.Values) config.LoadConfig();
                return "Loaded all Config Files";

            case "merge" when args.Length == 2:
                if (!bool.TryParse(args[1], out var enable))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "Usage: config merge (true/false)"
                    };

                configHolder.Config.ScenarioMerging = enable;
                configHolder.SaveConfig();
                return $"Set Scenario Merging to {enable}";

            case "merge" when args.Length == 1:
                return $"Scenario merging is set to {configHolder.Config.ScenarioMerging}";

            case "maxplayers" when args.Length == 2:
                if (!ushort.TryParse(args[1], out var amount))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "Usage: config maxplayers (number)"
                    };

                configHolder.Config.MaxPlayers = amount;
                configHolder.SaveConfig();
                foreach (var player in playerManager.Players)
                    player.Disconnect();
                return $"Set Max Players to {amount}";

            case "maxplayers" when args.Length == 1:
                return $"Max Players is set to {configHolder.Config.MaxPlayers}";

            default:
                return new CommandResult
                {
                    ResultType = ResultType.MissingParameter,
                    Message = "Usage: scenario merge (true/false)"
                };
        }
    }
}