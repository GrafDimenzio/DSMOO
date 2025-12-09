using System.Net;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

[Analyze(Priority = 90)]
public class BanManager(ConfigHolder<BanList> holder, ConfigManager configManager, EventManager eventManager, ILogger logger) : Manager
{
    private void SaveBanList()
    {
        configManager.SaveConfig(holder.ConfigObject);
    }
    
    private BanList Config => (BanList)holder.ConfigObject;
    
    public bool Enabled
    {
        get => Config.Enabled;
        set
        {
            Config.Enabled = value;
            SaveBanList();
        }
    }
    
    public HashSet<string> IPs
    {
        get => Config.IPs;
        set
        {
            Config.IPs = value;
            SaveBanList();
        }
    }
    
    public HashSet<Guid> Profiles
    {
        get => Config.Profiles;
        set
        {
            Config.Profiles = value;
            SaveBanList();
        }
    }
    
    public HashSet<string> Stages
    {
        get => Config.Stages;
        set
        {
            Config.Stages = value;
            SaveBanList();
        }
    }
    
    public HashSet<sbyte> GameModes
    {
        get => Config.GameModes;
        set
        {
            Config.GameModes = value;
            SaveBanList();
        }
    }

    public bool IsPlayerBanned(IPlayer player)
    {
        if (player.IsDummy || player.Ip == null) return false;
        return IsIPv4Banned(player.Ip) || IsProfileBanned(player.Id);
    }
    
    public bool IsIPv4Banned(IPAddress? address) => IsIPv4Banned(address?.ToString() ?? string.Empty);
    public bool IsIPv4Banned(string ip) => IPs.Contains(ip);
    public bool IsProfileBanned(Guid id) => Profiles.Contains(id);
    public bool IsStageBanned(string stage) => Stages.Contains(stage);
    public bool IsGameModeBanned(GameMode gameMode) => GameModes.Contains((sbyte)gameMode);


    public void BanPlayer(IPlayer player)
    {
        if(player is Player realPlayer)
            realPlayer.IsBanned = true;
        player.Crash(true);
        if (player.Ip != null)
            BanIPv4(player.Ip.ToString());
        BanProfile(player.Id);
    }

    public bool BanIPv4(string address)
    {
        if (!IPs.Add(address)) return false;
        SaveBanList();
        return true;
    }

    public bool UnBanIPv4(IPAddress address)
    {
        if(!IPs.Contains(address.ToString())) return false;
        IPs.Remove(address.ToString());
        SaveBanList();
        return true;
    }

    public bool BanProfile(Guid id)
    {
        if (!Profiles.Add(id)) return false;
        SaveBanList();
        return true;
    }
    
    public bool UnBanProfile(Guid id)
    {
        if(!Profiles.Contains(id)) return false;
        Profiles.Remove(id);
        SaveBanList();
        return true;
    }

    public bool BanStage(string stage)
    {
        if (!Stages.Add(stage)) return false;
        SaveBanList();
        return true;
    }
    
    public bool UnBanStage(string stage)
    {
        if(!Stages.Contains(stage)) return false;
        Stages.Remove(stage);
        SaveBanList();
        return true;
    }

    public bool BanGameMode(GameMode gameMode)
    {
        if(!GameModes.Add((sbyte)gameMode)) return false;
        SaveBanList();
        return true;
    }
    
    public bool UnBanGameMode(GameMode gameMode)
    {
        if(!GameModes.Contains((sbyte)gameMode)) return false;
        GameModes.Remove((sbyte)gameMode);
        SaveBanList();
        return true;
    }

    public string GetSafeStage(string currentStage)
    {
        if(!IsStageBanned(currentStage)) return currentStage;
        var kingdom = MapInfo.AllKingdoms.FirstOrDefault(x => x.SubAreas.Any(y => y.SubAreaName == currentStage));
        //A Subarea is banned so send him back to the world the subarea belongs to
        if (kingdom != null && !IsStageBanned(kingdom.MainStageName))
        {
            return kingdom.MainStageName;
        }

        //Check if you can send him to any Kingdom
        var kingdoms = MapInfo.AllKingdoms.Where(x => x.MainStageName != "HomeShipInsideStage").ToList();
        foreach (var king in kingdoms)
        {
            if(!IsStageBanned(king.MainStageName))
                return king.MainStageName;
        }
        //All Kingdoms are banned. Send the player to any subarea
        foreach (var king in kingdoms)
        {
            foreach (var subArea in king.SubAreas)
            {
                if(!IsStageBanned(subArea.SubAreaName))
                    return subArea.SubAreaName;
            }
        }
        
        return !IsStageBanned("HomeShipInsideStage") ? "HomeShipInsideStage" : "";
    }

    public void SendPlayerToSafeStage(IPlayer player)
    {
        var newStage = GetSafeStage(player.Stage);
        player.ChangeStage(newStage, MapInfo.GetConnection(player.Stage, newStage, false));
    }

    public override void Initialize()
    {
        eventManager.OnPlayerChangeStage.Subscribe(PlayerChangeStage);
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        eventManager.OnPlayerPreJoin.Subscribe(OnPlayerPreJoin);
    }

    private void OnPlayerPreJoin(PlayerPreJoinEventArgs args)
    {
        if(!Enabled) return;
        if(!IsProfileBanned(args.Client.Id) && !IsIPv4Banned((args.Client.Socket.RemoteEndPoint as IPEndPoint)?.Address)) return;
        args.AllowJoin = false;
    }

    private void PlayerChangeStage(PlayerChangeStageEventArgs args)
    {
        if(!IsStageBanned(args.NewStage)) return;
        args.SendBack = true;
        if (!IsStageBanned(args.SendBackStage) && !string.IsNullOrWhiteSpace(args.SendBackStage)) return;
        args.SendBackStage = GetSafeStage(string.IsNullOrWhiteSpace(args.SendBackStage) ? args.NewStage : args.SendBackStage);
        args.SendBackScenario = -1;
        args.SendBackWarp = MapInfo.GetConnection(args.PreviousStage, args.SendBackStage, false);
    }

    public void OnPacket(PacketReceivedEventArgs args)
    {
        switch (args.Packet)
        {
            case TagPacket tagPacket:
                break;
        }
    }
}

[Analyze(Priority = 91)]
[Config(Name = "banlist")]
public class BanList : IConfig
{
    public bool Enabled { get; set; } = true;
    public HashSet<string> IPs { get; set; } = [];
    public HashSet<Guid> Profiles { get; set; } = [];
    public HashSet<string> Stages { get; set; } = [];
    public HashSet<sbyte> GameModes { get; set; } = [];
}