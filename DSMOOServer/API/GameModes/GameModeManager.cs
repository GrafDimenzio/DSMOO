using System.Collections.ObjectModel;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.GameModes.Hints;
using DSMOOServer.API.Player;
using DSMOOServer.API.Stage;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

public class GameModeManager(
    EventManager eventManager,
    StageManager stageManager,
    ConfigHolder<ServerMainConfig> configHolder,
    ConfigHolder<GameModeConfig> gameConfigHolder,
    Analyzer analyzer,
    ObjectController objectController,
    ILogger logger,
    PlayerManager playerManager) : Manager
{
    private readonly Dictionary<string, Type> _games = [];
    public readonly Dictionary<string, IHint> Hints = new();
    public ReadOnlyDictionary<string, Type> Games => _games.AsReadOnly();

    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyzeGames);
        analyzer.OnAnalyze.Subscribe(OnAnalyzeHint);
        eventManager.OnPlayerState.Subscribe(OnPlayerState);
    }

    public IGame? StartGame(string gameName, string players, string stageConfig, string hintConfig,
        params string[] args)
    {
        var stage = GetStageConfig(stageConfig);
        if (stage == null)
            return null;
        var hint = GetHintConfig(hintConfig);
        var playerSearch = playerManager.SearchForPlayers(players.Split(' '));
        return playerSearch.Players.Count == 0
            ? null
            : StartGame(gameName, playerSearch.Players.ToArray(), stage, hint, args);
    }

    public IGame? StartGame(string gameName, IPlayer[] players, StageConfig stageConfig, HintConfig hintConfig,
        params string[] arguments)
    {
        if (!_games.ContainsKey(gameName.ToLower()))
            return null;

        var game = (IGame?)objectController.CreateObject(_games[gameName.ToLower()]);
        if (game == null)
            return null;
        game.StartGame(players, stageConfig, hintConfig, arguments);
        return game;
    }

    public StageConfig? GetStageConfig(string configName)
    {
        foreach (var stageConfig in gameConfigHolder.Config.StageConfigs)
            if (string.Equals(stageConfig.Name, configName, StringComparison.OrdinalIgnoreCase))
                return stageConfig;

        var stage = stageManager.GetStageFromInput(configName);
        if (stage == null)
            return null;

        return new StageConfig
        {
            Name = configName,
            AllowAll = false,
            AllowedStages = [stage],
            StartingStage = [stage],
            WaitingStage = [stage]
        };
    }

    public HintConfig? GetHintConfig(string configName)
    {
        foreach (var hintConfig in gameConfigHolder.Config.HintConfigs)
            if (string.Equals(hintConfig.Name, configName, StringComparison.OrdinalIgnoreCase))
                return hintConfig;

        if (configName.ToLower() == "none")
            return new HintConfig()
            {
                Name = "None",
                Hints = [],
                UpdateOldHintOnNewOnes = false,
            };

        return null;
    }

    public HintData GetHint(string[] hintTypes, IPlayer player, IGame? forGame = null)
    {
        var hints = new List<string>();
        var types = new List<string>();

        foreach (var hintType in hintTypes)
        {
            if (!Hints.TryGetValue(hintType, out var hintGenerator))
                continue;
            hints.Add(hintGenerator.GetHint(player));
            types.Add(hintType);
        }

        var hintData = new HintData
        {
            Player = player,
            HintTypes = types.ToArray(),
            HintTexts = hints.ToArray()
        };
        var ev = new GetHintEventArgs { HintData = hintData, Game = forGame };
        eventManager.OnGetHint.RaiseEvent(ev);
        logger.Info($"Hint about {ev.HintData.Player.Name}: {string.Join(' ', ev.HintData.HintTexts)}");
        return ev.HintData;
    }

    public void SendHintToPlayer(IPlayer player, HintData hintData)
    {
        var ev = new SendHintEventArgs
        {
            Hint = hintData,
            SendToPlayer = player
        };
        eventManager.OnSendHint.RaiseEvent(ev);
    }

    private void OnAnalyzeGames(AnalyzeEventArgs args)
    {
        if (!args.Is<IGame>()) return;
        var names = args.GetAttribute<GameAttribute>()?.Names;
        if (names == null) return;
        foreach (var name in names) _games[name.ToLower()] = args.Type;
    }

    private void OnAnalyzeHint(AnalyzeEventArgs args)
    {
        if (!args.Is<IHint>()) return;
        var name = args.GetAttribute<HintAttribute>()?.Name;
        if (name == null) return;
        var hintObject = objectController.GetObject(args.Type);
        if (hintObject is not IHint hint) return;
        Hints[name] = hint;
    }

    private void OnPlayerState(PlayerStateEventArgs args)
    {
        if (configHolder.Config.HidersCanSeeEachOther || args.Player.IsIt ||
            args.Player.CurrentGameMode == GameMode.None) return;

        foreach (var realPlayer in playerManager.RealPlayers)
            if (!realPlayer.IsIt && realPlayer.CurrentGameMode != GameMode.None)
                args.SpecificInvisible.Add(realPlayer.Id);
    }
}

[Config(Name = "gameModes")]
public class GameModeConfig : IConfig
{
    public StageConfig[] StageConfigs { get; set; } =
    [
        new()
        {
            Name = "cap",
            StartingStage = ["CapWorldHomeStage"],
            AllowedStages =
            [
                "CapWorldHomeStage", "CapWorldTowerStage", "PoisonWaveExStage", "PushBlockExStage", "FrogSearchExStage",
                "RollingExStage"
            ],
            WaitingStage = ["HomeShipInsideStage", "SeaWorldLighthouseZone", "SeaWorldLavaZone"]
        },
        new()
        {
            Name = "cap-nosub",
            StartingStage = ["CapWorldHomeStage"],
            AllowedStages = ["CapWorldHomeStage"],
            WaitingStage = ["HomeShipInsideStage", "SeaWorldLighthouseZone", "SeaWorldLavaZone"]
        },
        new()
        {
            Name = "cap-all",
            StartingStage = ["CapWorldHomeStage"],
            AllowAll = true,
            WaitingStage = ["HomeShipInsideStage", "SeaWorldLighthouseZone", "SeaWorldLavaZone"]
        }
    ];

    public HintConfig[] HintConfigs { get; set; } =
    [
        new()
        {
            Name = "default",
            Hints =
            [
                new HintConfig.Hint
                {
                    Time = 300,
                    HintType = "Area"
                },
                new HintConfig.Hint
                {
                    Time = 600,
                    HintType = "MapNorthSouth"
                },
                new HintConfig.Hint
                {
                    Time = 900,
                    HintType = "MapEastWest"
                },
                new HintConfig.Hint
                {
                    Time = 1200,
                    HintType = "MapNumber"
                },
                new HintConfig.Hint
                {
                    Time = 1500,
                    HintType = "MapLetter"
                },
                new HintConfig.Hint
                {
                    Time = 1800,
                    HintType = "MapCell"
                },
                new HintConfig.Hint
                {
                    Time = 2100,
                    HintType = "Position"
                }
            ]
        }
    ];
}