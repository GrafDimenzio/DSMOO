using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using DSMOOServer.Connection;
using DSMOOServer.Helper;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Player;

/// <summary>
/// The Player class is intended to be used as a abstract more easily usable Class.
/// The Client can be null for Player's that are don't have a connection like a bot
/// </summary>
/// <param name="client"></param>
public class Player : IPlayer
{
    public Player(Client client)
    {
        Client = client;
    }
    
    public Client Client { get; set; }

    public IPAddress? Ip => Client.Socket.RemoteEndPoint is not IPEndPoint ip ? null : ip.Address;
    public Guid Id => Client.Id;
    public bool IsDummy => false;
    public string Name => Client.Name;

    public Vector3 Position { get; internal set; } = Vector3.Zero;
    public Quaternion Rotation { get; internal set; } = Quaternion.Identity;
    public ushort Act { get; internal set; } = 0;
    public ushort SubAct { get; internal set; } = 0;
    public float[] AnimationBlendWeights { get; internal set; } = [0];

    public CostumePacket Costume { get; internal set; } = new CostumePacket { BodyName = "", CapName = "" };
    public string Capture { get; internal set; } = "";
    
    public bool Is2d { get; internal set; } = false;
    public byte Scenario { get; internal set; } = 0;
    public string Stage { get; internal set; } = "";

    public GameMode CurrentGameMode { get; internal set; } = GameMode.None;
    public bool IsIt { get; internal set; } = false;

    public Time Time { get; internal set; } = new Time(0, 0, DateTime.Now);
    
    public bool IsSaveLoaded { get; internal set; } = false;
    
    public bool IsBanned { get; } = false;
    
    public bool DisableMoonSync { get; set; } = false;
    
    public ConcurrentBag<int> SyncedMoons { get; } = [];

    public void Disconnect()
    {
        Client.Socket.Disconnect(false);
    }

    public void Crash(bool ban)
    {
        Send(new ChangeStagePacket()
        {
            Stage = ban ? "$ejected" : "$agogusStage",
        }, null).GetAwaiter().GetResult();
        Client.Ignored = true;
    }

    public void SendShine(int id)
    {
        Client.Send(new ShinePacket()
        {
            ShineId = id
        });
    }

    public void ChangeStage(string stage, string warp, sbyte scenario = 0, byte subScenarioType = 0)
    {
        Client.Send(new ChangeStagePacket()
        {
            Stage = stage,
            Id = warp,
            Scenario = scenario,
            SubScenarioType = subScenarioType
        });
    }

    public async Task Send<T>(T packet, Guid? sender) where T : struct, IPacket
    {
        await Client.Send(packet, sender);
    }

    public void CopyDataFromOtherPlayer(IPlayer player)
    {
        Position = player.Position;
        Rotation = player.Rotation;
        Act = player.Act;
        SubAct = player.SubAct;
        AnimationBlendWeights = player.AnimationBlendWeights;
        Costume = player.Costume;
        Capture = player.Capture;
        Is2d = player.Is2d;
        Scenario = player.Scenario;
        Stage = player.Stage;
        CurrentGameMode = player.CurrentGameMode;
        IsIt = player.IsIt;
        Time = player.Time;
        IsSaveLoaded = player.IsSaveLoaded;
    }
}