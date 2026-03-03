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
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

public class GameModeManager(
    EventManager eventManager,
    ConfigHolder<ServerMainConfig> configHolder,
    ConfigHolder<GameModeConfig> gameConfigHolder,
    Analyzer analyzer,
    ObjectController objectController,
    ILogger logger,
    PlayerManager playerManager) : Manager
{
    private readonly List<Type> _games = [];
    public Dictionary<string, IHint> Hints = new();
    public ReadOnlyCollection<Type> Games => _games.AsReadOnly();

    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
        eventManager.OnPlayerState.Subscribe(OnPlayerState);
    }

    public HintData GetHint(string hintType, IPlayer player, IGame? forGame = null)
    {
        var hintGenerator = Hints[hintType];
        var hintString = hintGenerator.GetHint(player);
        var hintData = new HintData
        {
            Player = player,
            HintName = hintType,
            HintText = hintString
        };
        var ev = new GetHintEventArgs { HintData = hintData, Game = forGame };
        eventManager.OnGetHint.RaiseEvent(ev);
        logger.Info($"Hint about {ev.HintData.Player.Name}-{ev.HintData.HintName}: {ev.HintData.HintText}");
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

    private void OnAnalyze(AnalyzeEventArgs args)
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