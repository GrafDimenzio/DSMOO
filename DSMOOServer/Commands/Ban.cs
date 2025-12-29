using System.Net;
using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.API.Stage;
using DSMOOServer.Helper;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "ban",
    Aliases = ["unban"],
    Description = "Command for managing bans",
    Parameters = ["[list/enable/disable/player/profile/ip/stage/gamemode]"]
)]
public class Ban(BanManager manager, PlayerManager playerManager) : Command
{
    private readonly Dictionary<string, string> _usages = new()
    {
        { "player", "player <* | !* (usernames to not ban...) | (usernames to ban...)>" },
        { "profile", "profile <profile-id>" },
        { "ip", "ip <ipv4-address>" },
        { "stage", "stage <stage-name>" },
        { "gamemode", "gamemode <gamemode>" }
    };

    public override CommandResult Execute(string command, string[] args)
    {
        var banMode = command.Equals("ban", StringComparison.CurrentCultureIgnoreCase);
        var subcommands = $"[{(banMode ? "list/enable/disable/player/" : "")}profile/ip/stage/gamemode]";
        if (args.Length == 0)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "You need to specify a sub command " + subcommands
            };


        var hasParameter = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]);

        switch (args[0].ToLower())
        {
            case "player" when !hasParameter && banMode:
            case "profile" when !hasParameter:
            case "ip" when !hasParameter:
            case "stage" when !hasParameter:
            case "gamemode" when !hasParameter:
                return new CommandResult
                {
                    ResultType = ResultType.MissingParameter,
                    Message = $"Usage: {(banMode ? "ban" : "unban")} " + _usages[args[0].ToLower()]
                };

            case "list" when banMode:
                var msg = new StringBuilder();
                msg.Append($"Banlist: {(manager.Enabled ? "enabled" : "disabled")}");

                var join = "\n  - ";

                if (manager.IPs.Count > 0)
                {
                    msg.Append("\nBanned I4 adresses:" + join);
                    msg.Append(string.Join(join, manager.IPs));
                }

                if (manager.Profiles.Count > 0)
                {
                    msg.Append("\nBanned Profiles:" + join);
                    msg.Append(string.Join(join, manager.Profiles));
                }

                if (manager.Stages.Count > 0)
                {
                    msg.Append("\nBanned Stages:" + join);
                    msg.Append(string.Join(join, manager.Stages));
                }

                if (manager.GameModes.Count > 0)
                {
                    msg.Append("\nBanned Gamemodes:" + join);
                    msg.Append(string.Join(join, manager.GameModes.Select(x => (GameMode)x)));
                }

                return msg.ToString();

            case "enable" when banMode:
                manager.Enabled = true;
                return "Banlist enabled";

            case "disable" when banMode:
                manager.Enabled = false;
                return "Banlist disabled";

            case "player" when banMode:
                var players = playerManager.SearchForPlayers(args[1..]);
                foreach (var player in players.Players)
                    manager.BanPlayer(player);
                return MessageHelper.FormatMessage(players, "Banned");

            case "profile":
                if (!Guid.TryParse(args[1], out var id))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "You need to specify a valid profile id"
                    };
                if (banMode)
                {
                    foreach (var player in playerManager.SearchForPlayers([args[1]]).Players)
                        player.Crash(true);
                    manager.BanProfile(id);
                    return "Banned profile " + id;
                }

                manager.UnBanProfile(id);
                return "Unbanned profile " + id;

            case "ip":
                if (!IPAddress.TryParse(args[1], out var ip))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "You need to specify a valid ip address"
                    };

                if (banMode)
                {
                    foreach (var player in playerManager.SearchForPlayers([args[1]]).Players)
                        player.Crash(true);
                    manager.BanIPv4(ip.ToString());
                    return "Banned ip " + ip;
                }

                manager.UnBanIPv4(ip);
                return "Unbanned ip " + ip;

            case "stage":
                var stage = Stages.Input2Stage(args[1]);
                if (stage == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "You need to specify a valid stage"
                    };
                if (banMode)
                {
                    manager.BanStage(stage);
                    foreach (var player in playerManager.RealPlayers)
                        if (player.Stage == stage)
                            manager.SendPlayerToSafeStage(player);
                    return "Banned Stage " + stage;
                }

                manager.UnBanStage(stage);
                return "Unbanned stage " + stage;


            case "gamemode":
                if (!Enum.TryParse<GameMode>(args[1], out var gameMode))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "You need to specify a valid gamemode"
                    };
                if (banMode)
                {
                    manager.BanGameMode(gameMode);
                    return "Banned gamemode " + gameMode;
                }

                manager.UnBanGameMode(gameMode);
                return "Unbanned gamemode " + gameMode;
        }

        return new CommandResult
        {
            ResultType = ResultType.InvalidParameter,
            Message = "Invalid sub command use one of those " + subcommands
        };
    }
}