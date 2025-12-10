using System.Collections.Concurrent;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

[Analyze(Priority = 1)]
public class MoonManager(EventManager eventManager, ILogger logger, ConfigHolder<ServerMainConfig> configHolder, Server server, ConfigHolder<MoonList> moonListHolder, ConfigManager configManager) : Manager
{
    public ConcurrentBag<int> MoonSync = new();
    public SortedSet<int> ExcludedMoons => Config.ExcludedMoons;

    private System.Timers.Timer _timer = new(120000);
    
    private ServerMainConfig Config { get; } = configHolder.Config;

    public async Task AddMoon(int moonId)
    {
        MoonSync.Add(moonId);
        SaveMoons();
        await SyncMoon(moonId);
    }

    public void SaveMoons()
    {
        moonListHolder.Config.Moons = new HashSet<int>(MoonSync);
        configManager.SaveConfig(moonListHolder.Config);
    }

    public async Task SyncMoons()
    {
        await Parallel.ForEachAsync(server.Clients, async(client, _) => await SyncMoonsForClient(client));
    }

    public async Task SyncMoon(int moonId)
    {
        await Parallel.ForEachAsync(server.Clients, async (client, _) => await SyncMoonForClient(client, moonId));
    }

    public async Task SyncMoonForClient(Client client, int moonId)
    {
        if (!Config.MoonSyncEnabled || client.Player.DisableMoonSync || !client.FirstPacketSend) return;
        if (Config.ExcludedMoons.Contains(moonId) || client.Player.SyncedMoons.Contains(moonId)) return;
        await client.Send(new ShinePacket()
        {
            ShineId = moonId
        });
        client.Player.SyncedMoons.Add(moonId);
    }

    public async Task SyncMoonsForClient(Client client)
    {
        foreach (var moon in MoonSync)
        {
            await SyncMoonForClient(client, moon);
        }
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
            case ShinePacket shinePacket:
                if (Config.ExcludedMoons.Contains(shinePacket.ShineId))
                {
                    logger.Info($"{args.Sender.Name} collected shine {shinePacket.ShineId} that is disabled by the config");
                    return;
                }

                if (!args.Sender.Player.IsSaveLoaded) return;
                args.Sender.Logger.Info($"Got Moon {shinePacket.ShineId}");
                var ev = new PlayerCollectMoonEventArgs()
                {
                    Player = args.Sender.Player,
                    Moon = shinePacket.ShineId
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
                                await SyncMoonsForClient(args.Sender);
                            });
                            break;
                }
                break;
            
            case CostumePacket:
                //When loading a different Save the Client will send a CostumePacket so this is just to be sure I assume
                Task.Run(() => SyncMoonsForClient(args.Sender));
                break;
        }
        
    }
    
}

[Config(Name = "MoonList")]
public class MoonList : IConfig
{
    public HashSet<int> Moons { get; set; } = [];
}