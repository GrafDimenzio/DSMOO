using System.Collections.Concurrent;
using DSMOOFramework.Config;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
using DSMOOServer.Network.Packets;
using Timer = System.Timers.Timer;

namespace DSMOOServer.Logic;

public class MoonManager(
    EventManager eventManager,
    ILogger logger,
    ConfigHolder<ServerMainConfig> configHolder,
    PlayerManager playerManager,
    ConfigHolder<MoonList> moonListHolder) : Manager
{
    private readonly Timer _timer = new(120000);
    public ConcurrentBag<int> MoonSync = new();
    public SortedSet<int> ExcludedMoons => Config.ExcludedMoons;

    private ServerMainConfig Config { get; } = configHolder.Config;

    public bool Enable
    {
        get => Config.MoonSyncEnabled;
        set
        {
            Config.MoonSyncEnabled = value;
            moonListHolder.SaveConfig();
        }
    }

    public async Task AddMoon(int moonId)
    {
        MoonSync.Add(moonId);
        SaveMoons();
        await SyncMoon(moonId);
    }

    public void SaveMoons()
    {
        moonListHolder.Config.Moons = new HashSet<int>(MoonSync);
        moonListHolder.SaveConfig();
    }

    public async Task SyncMoons()
    {
        await Parallel.ForEachAsync(playerManager.Players, async (player, _) => await SyncMoonsForPlayer(player));
    }

    public async Task SyncMoon(int moonId)
    {
        await Parallel.ForEachAsync(playerManager.Players,
            async (player, _) => await SyncMoonForPlayer(player, moonId));
    }

    public async Task SyncMoonForPlayer(IPlayer player, int moonId)
    {
        if (player is not Player ply)
            return;

        if (!Config.MoonSyncEnabled || ply.DisableMoonSync) return;
        if (Config.ExcludedMoons.Contains(moonId) || ply.SyncedMoons.Contains(moonId)) return;
        await player.Send(new MoonPacket
        {
            MoonId = moonId
        });
        ply.SyncedMoons.Add(moonId);
    }

    public async Task SyncMoonsForPlayer(IPlayer player)
    {
        foreach (var moon in MoonSync)
            await SyncMoonForPlayer(player, moon);
    }

    public override void Initialize()
    {
        MoonSync = new ConcurrentBag<int>(moonListHolder.Config.Moons);
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Elapsed += async (_, _) => { await SyncMoons(); };
        _timer.Start();
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        if (!Config.MoonSyncEnabled) return;

        switch (args.Packet)
        {
            case MoonPacket shinePacket:
                if (Config.ExcludedMoons.Contains(shinePacket.MoonId))
                {
                    logger.Info(
                        $"{args.Sender.Name} collected shine {shinePacket.MoonId} that is disabled by the config");
                    return;
                }

                if (!args.Sender.Player.IsSaveLoaded) return;
                args.Sender.Logger.Info($"Got Moon {shinePacket.MoonId}");
                var ev = new PlayerCollectMoonEventArgs
                {
                    Player = args.Sender.Player,
                    Moon = shinePacket.MoonId
                };
                eventManager.OnPlayerCollectMoon.RaiseEvent(ev);
                Task.Run(() => AddMoon(ev.Moon));
                break;

            case GamePacket gamePacket:
                switch (gamePacket.Stage)
                {
                    case "CapWorldHomeStage" when gamePacket.ScenarioNum == 1:
                    case "CapWorldTowerStage" when gamePacket.ScenarioNum == 1:
                        if (args.Sender.Player.DisableMoonSync) return;
                        args.Sender.Player.DisableMoonSync = true;
                        args.Sender.Player.SyncedMoons.Clear();
                        args.Sender.Logger.Info("Entered Cap on new save, preventing moon sync until Cascade");
                        if (Config.ClearOnNewSaves && MoonSync.Count > 0)
                        {
                            MoonSync.Clear();
                            args.Sender.Logger.Info("Cleared saved moons due to new save");
                            Task.Run(SaveMoons);
                        }

                        break;

                    default:
                        if (!args.Sender.Player.DisableMoonSync) return;
                        Task.Run(async () =>
                        {
                            args.Sender.Logger.Info(
                                "Entered Cascade or later with moon sync disabled, enabling moon sync again");
                            await Task.Delay(2000);
                            args.Sender.Player.DisableMoonSync = false;
                            await SyncMoonsForPlayer(args.Sender.Player);
                        });
                        break;
                }

                break;

            case CostumePacket:
                //When loading a different Save the Client will send a CostumePacket so this is just to be sure I assume
                Task.Run(() => SyncMoonsForPlayer(args.Sender.Player));
                break;
        }
    }
}

[Config(Name = "moons")]
public class MoonList : IConfig
{
    public HashSet<int> Moons { get; set; } = [];
}