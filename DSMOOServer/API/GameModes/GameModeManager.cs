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

    public IGame? StartGame(string gameName, IPlayer[] players, StagePreset stagePreset, HintPreset hintPreset,
        params string[] arguments)
    {
        if (!_games.ContainsKey(gameName.ToLower()))
            return null;

        var game = (IGame?)objectController.CreateObject(_games[gameName.ToLower()]);
        if (game == null)
            return null;
        game.StartGame(players, stagePreset, hintPreset, arguments);
        return game;
    }

    public StagePreset? GetStageConfig(string configName)
    {
        foreach (var stageConfig in gameConfigHolder.Config.StageConfigs)
            if (string.Equals(stageConfig.Name, configName, StringComparison.OrdinalIgnoreCase))
                return stageConfig;

        var stage = stageManager.GetStageFromInput(configName);
        if (stage == null)
            return null;

        return new StagePreset
        {
            Name = configName,
            AllowAll = false,
            AllowedStages = [stage],
            StartingStages = [stage],
            WaitingStages = [stage]
        };
    }

    public HintPreset? GetHintConfig(string configName)
    {
        foreach (var hintConfig in gameConfigHolder.Config.HintConfigs)
            if (string.Equals(hintConfig.Name, configName, StringComparison.OrdinalIgnoreCase))
                return hintConfig;

        if (configName.ToLower() == "none")
            return new HintPreset
            {
                Name = "None",
                Hints = [],
                UpdateOldHintOnNewOnes = false
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
    public StagePreset[] StageConfigs { get; set; } =
    [
        new()
        {
            Name = "cap",
            StartingStages = ["CapWorldHomeStage"],
            AllowedStages =
            [
                "CapWorldTowerStage", "PoisonWaveExStage", "PushBlockExStage", "FrogSearchExStage",
                "RollingExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cap-nosub",
            StartingStages = ["CapWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cap-all",
            StartingStages = ["CapWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cascade",
            StartingStages = ["WaterfallWorldHomeStage"],
            AllowedStages =
            [
                "WindBlowExStage", "Lift2DExStage", "TrexPoppunExStage",
                "WanwanClashExStage",
                "CapAppearExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cascade-nosub",
            StartingStages = ["WaterfallWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cascade-all",
            StartingStages = ["WaterfallWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "sand",
            StartingStages = ["SandWorldHomeStage"],
            AllowedStages =
            [
                "SandWorldCostumeStage", "MeganeLiftExStage", "RocketFlowerExStage",
                "SandWorldPressExStage", "SandWorldKillerExStage", "SandWorldPyramid000Stage",
                "SandWorldPyramid001Stage", "SandWorldShopStage", "SandWorldSecretStage", "SandWorldRotateExStage",
                "SandWorldSlotStage", "SandWorldVibrationStage", "SandWorldUnderground000Stage",
                "SandWorldUnderground001Stage", "WaterTubeExStage", "SandWorldMeganeExStage", "SandWorldSphinxExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "sand-nosub",
            StartingStages = ["SandWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "sand-all",
            StartingStages = ["SandWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lake",
            StartingStages = ["LakeWorldHomeStage"],
            AllowedStages =
            [
                "GotogotonExStage", "FastenerExStage", "FrogPoisonExStage", "TrampolineWallCatchExStage",
                "LakeWorldShopStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lake-nosub",
            StartingStages = ["LakeWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lake-all",
            StartingStages = ["LakeWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "wooded",
            StartingStages = ["ForestWorldHomeStage"],
            AllowedStages =
            [
                "ForestWorldCloudBonusExStage", "AnimalChaseExStage", "FogMountainExStage", "ForestWorldBossStage",
                "PackunPoisonExStage", "KillerRoadExStage", "ForestWorldWaterExStage", "ForestWorldBonusStage",
                "ShootingElevatorExStage", "ForestWorldTowerStage", "RailCollisionExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "wooded-nosub",
            StartingStages = ["ForestWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "wooded-all",
            StartingStages = ["ForestWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "deepwoods",
            StartingStages = ["ForestWorldWoodsStage"],
            AllowedStages =
            [
                "ForestWorldWoodsCostumeStage", "ForestWorldWoodsTreasureStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cloud",
            StartingStages = ["CloudWorldHomeStage"],
            AllowedStages =
            [
                "Cube2DExStage", "FukuwaraiKuriboStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cloud-nosub",
            StartingStages = ["CloudWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "cloud-all",
            StartingStages = ["CloudWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lost",
            StartingStages = ["ClashWorldHomeStage"],
            AllowedStages =
            [
                "ClashWorldShopStage", "JangoExStage", "ImomuPoisonExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lost-nosub",
            StartingStages = ["ClashWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "lost-all",
            StartingStages = ["ClashWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "metro",
            StartingStages = ["ClashWorldHomeStage"],
            AllowedStages =
            [
                "BikeSteelExStage", "CityWorldFactoryStage", "CapRotatePackunExStage", "CityPeopleRoadStage",
                "ShootingCityExStage", "PoleGrabCeilExStage", "Theater2DExStage", "DonsukeExStage",
                "CityWorldMainTowerStage", "CityWorldShop01Stage", "Note2D3DRoomExStage", "PoleKillerExStage",
                "RadioControlExStage", "SwingSteelExStage", "ElectricWireExStage", "CityWorldSandSlotStage",
                "TrexBikeExStage"
            ],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "metro-nosub",
            StartingStages = ["ClashWorldHomeStage"],
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
        new()
        {
            Name = "metro-all",
            StartingStages = ["ClashWorldHomeStage"],
            AllowAll = true,
            WaitingStages =
            [
                "HomeShipInsideStage", "CityWorldFactory01Zone", "LakeWorldTownZone", "SeaWorldLighthouseZone",
                "SeaWorldLavaZone"
            ]
        },
    ];

    public HintPreset[] HintConfigs { get; set; } =
    [
        new()
        {
            Name = "default",
            Hints =
            [
                new HintPreset.Hint
                {
                    Time = 300,
                    HintType = "Area"
                },
                new HintPreset.Hint
                {
                    Time = 600,
                    HintType = "MapNorthSouth"
                },
                new HintPreset.Hint
                {
                    Time = 900,
                    HintType = "MapEastWest"
                },
                new HintPreset.Hint
                {
                    Time = 1200,
                    HintType = "MapNumber"
                },
                new HintPreset.Hint
                {
                    Time = 1500,
                    HintType = "MapLetter"
                },
                new HintPreset.Hint
                {
                    Time = 1800,
                    HintType = "MapCell"
                },
                new HintPreset.Hint
                {
                    Time = 2100,
                    HintType = "Position"
                }
            ]
        }
    ];
}