using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using System.Text;
using DSMOOFramework.Controller;
using DSMOOServer.API.Enum;
using DSMOOServer.Connection;
using DSMOOServer.Helper;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Player;

/// <summary>
///     The Player class is intended to be used as a abstract more easily usable Class.
///     The Client can be null for Player's that are don't have a connection like a bot
/// </summary>
/// <param name="client"></param>
public class Player(Client client, ObjectController objectController) : IPlayer
{
    private readonly Dictionary<Type, PlayerComponent> _components = [];
    private readonly PlayerManager _playerManager = objectController.GetObject<PlayerManager>()!;
    private readonly Server _server = objectController.GetObject<Server>()!;

    public Client Client { get; internal set; } = client;

    public bool DisableMoonSync { get; set; } = false;

    public ConcurrentBag<int> SyncedMoons { get; } = [];

    public IPAddress? Ip => Client.Socket.RemoteEndPoint is not IPEndPoint ip ? null : ip.Address;
    public Guid Id => Client.Id;
    public bool IsDummy => false;
    public string Name => Client.Name;

    public Vector3 Position { get; internal set; } = Vector3.Zero;
    public Quaternion Rotation { get; internal set; } = Quaternion.Identity;
    public ushort Act { get; internal set; }
    public ushort SubAct { get; internal set; }
    public float[] AnimationBlendWeights { get; internal set; } = [0];

    public CostumePacket Costume { get; internal set; } = new() { BodyName = "", CapName = "" };
    public string Capture { get; internal set; } = "";

    public bool Is2d { get; internal set; }
    public byte Scenario { get; internal set; }
    public string Stage { get; internal set; } = "";

    public GameMode CurrentGameMode { get; internal set; } = GameMode.None;
    public bool IsIt { get; internal set; }

    public Time Time { get; internal set; } = new(0, 0, DateTime.Now);

    public bool IsSaveLoaded { get; internal set; }

    public bool IsBanned => Client.IsBanned;

    public PlayerAction LastPlayerAction { get; set; }

    public void Disconnect()
    {
        try
        {
            Client.Socket?.Disconnect(false);
        }
        catch (ObjectDisposedException)
        {
            //Ignore when already disposed
        }
    }

    public Task Crash(bool ban)
    {
        return Client.Crash(ban);
    }

    public Task SendShine(int id)
    {
        return Client.Send(new MoonPacket
        {
            MoonId = id
        });
    }

    public Task ChangeStage(string stage, string warp, sbyte scenario = 0, byte subScenarioType = 0, int delay = 0)
    {
        return AsyncChangeStage(stage, warp, scenario, subScenarioType, delay);
    }

    public Task ChangeGameState(GameMode gameMode, bool isIt)
    {
        return AsyncChangeGameState(gameMode, isIt);
    }

    public Task ChangeGameTime(Time time)
    {
        return AsyncChangeGameTime(time);
    }

    public async Task Send<T>(T packet, Guid? sender) where T : struct, IPacket
    {
        await Client.Send(packet, sender);
    }

    public T? GetComponent<T>() where T : PlayerComponent
    {
        if (_components.TryGetValue(typeof(T), out var component))
            return (T)component;
        return null;
    }

    public T AddComponent<T>() where T : PlayerComponent
    {
        if (_components.TryGetValue(typeof(T), out var component))
            return (T)component;
        var comp = objectController.CreateObject<T>()!;
        comp.Player = this;
        _components[typeof(T)] = comp;
        return comp;
    }

    private async Task AsyncChangeGameState(GameMode gameMode, bool isIt)
    {
        var packet = new TagPacket
        {
            IsIt = isIt,
            GameMode = gameMode,
            UpdateType = TagPacket.TagUpdate.State
        };
        await _server.Broadcast(packet, Id);
        await Send(packet, Id);
        CurrentGameMode = gameMode;
        IsIt = isIt;
    }

    private async Task AsyncChangeGameTime(Time time)
    {
        var packet = new TagPacket
        {
            Seconds = time.Seconds,
            Minutes = time.Minutes,
            UpdateType = TagPacket.TagUpdate.Time
        };
        await _server.Broadcast(packet, Id);
        await Send(packet, Id);
        Time = time;
    }

    private async Task AsyncChangeStage(string stage, string warp, sbyte scenario = 0, byte subScenarioType = 0,
        int delay = 0)
    {
        if (Encoding.UTF8.GetBytes(warp).Length > Constants.WarpIdSize) warp = "";

        if (delay > 0)
            await Task.Delay(delay);
        //Scenario is 255 during transitions, so we need to wait
        while (Scenario == 255) await Task.Delay(100);

        await Client.Send(new ChangeStagePacket
        {
            Stage = stage,
            Id = warp,
            Scenario = scenario,
            SubScenarioType = subScenarioType
        });
    }
}