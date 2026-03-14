using System.Text;
using DSMOOFramework.Commands;
using DSMOOFramework.Config;
using DSMOOServer.API.GameModes;
using DSMOOServer.Helper;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "game",
    Aliases = ["gamemode", "games"],
    Description = "Starts or ends a gamemode.",
    Parameters = ["start/end/list"]
)]
public class Game(
    GameModeManager gameModeManager,
    PlayerManager playerManager,
    ConfigHolder<GameModeConfig> gameConfigHolder) : Command
{
    public override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        if (args.Length == 0)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Please specify one of those subcommands: start/end/list"
            };

        switch (args[0].ToLower())
        {
            case "start":
            case "begin":
                if (args.Length < 5)
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message =
                            "usage: game start (game) (stage preset) (hint preset) (player names in \" or * for everyone) (additional arguments for hide and seek you can specify waiting time and how many players should search)" +
                            "\n Use game list for a list of all games, stage presets and hint presets." +
                            "\n Use none as hint preset to disable hints" +
                            "\n Example usage: game start hide&seek cap none * 60 2 (this means all players participate, 2 players search and have to wait 60 seconds) or game start hide&seek cap-nosub default \"mario luigi\""
                    };
                if (!gameModeManager.GameTypes.ContainsKey(args[1].ToLower()))
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No Game with this name was found"
                    };
                var stage = gameModeManager.GetStageConfig(args[2]);
                if (stage == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No Stage Preset with that name was found"
                    };
                var hint = gameModeManager.GetHintConfig(args[3]);
                if (hint == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No Hint Preset with that name was found"
                    };
                var playerSearch = playerManager.SearchForPlayers(args[4].Split(' '));
                if (playerSearch.Players.Count == 0)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No Players were found"
                    };
                var newGame = gameModeManager.StartGame(args[1].ToLower(), playerSearch.Players.ToArray(), stage, hint,
                    args.Length >= 6 ? args[5..] : []);
                return newGame == null
                    ? "Couldn't start the game"
                    : MessageHelper.FormatMessage(playerSearch, $"Started a round of {newGame.DisplayName} for ");

            case "end":
            case "stop:":
                if (gameModeManager.ActiveGame == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.Error,
                        Message = "No Game is currently active"
                    };
                var name = gameModeManager.ActiveGame.DisplayName;
                gameModeManager.EndCurrentGame();
                return $"Ended game of {name}";

            case "list":
                var message = new StringBuilder();
                message.AppendLine("Installed Games:");
                foreach (var game in gameModeManager.GameTypes.Keys)
                    message.AppendLine($"    - {game}");

                message.AppendLine("Stage Presets:");
                foreach (var stageConfig in gameConfigHolder.Config.StageConfigs)
                    message.AppendLine($"    - {stageConfig.Name}");

                message.AppendLine("Hint Presets");
                foreach (var hintPreset in gameConfigHolder.Config.HintConfigs)
                    message.AppendLine($"    - {hintPreset.Name}");
                return message.ToString();

            default:
                return new CommandResult
                {
                    ResultType = ResultType.InvalidParameter,
                    Message = "Please specify one of those subcommands: start/end/list"
                };
        }
    }
}