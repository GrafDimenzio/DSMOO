using System.Buffers;
using System.Net;
using System.Numerics;
using DSMOOFramework.Controller;
using DSMOOServer.API.Enum;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Connection;
using DSMOOServer.Helper;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Player;

public class Dummy : IPlayer, IDisposable
{
    private readonly Dictionary<Type, PlayerComponent> _components = [];
    private readonly EventManager _eventManager;
    private readonly JoinManager _joinManager;
    private readonly ObjectController _objectController;
    private readonly PacketManager _packetManager;
    private readonly PlayerManager _playerManager;
    private readonly Server _server;

    public Dummy(Server server, PlayerManager playerManager, EventManager eventManager, JoinManager joinManager,
        PacketManager packetManager, ObjectController objectController)
    {
        _server = server;
        _playerManager = playerManager;
        _eventManager = eventManager;
        _joinManager = joinManager;
        _packetManager = packetManager;
        _objectController = objectController;
        _playerManager.Players.Add(this);
    }

    public void Dispose()
    {
        BroadcastPacket(new DisconnectPacket()).GetAwaiter().GetResult();
        _playerManager.Players.Remove(this);
    }

    public IPAddress? Ip { get; } = null;
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsDummy => true;
    public string Name { get; set; } = "Dummy";
    public Vector3 Position { get; } = Vector3.Zero;
    public Quaternion Rotation { get; } = Quaternion.Identity;
    public ushort Act { get; } = 0;
    public ushort SubAct { get; } = 0;
    public float[] AnimationBlendWeights { get; } = [];

    public CostumePacket Costume { get; } = new()
    {
        BodyName = "Mario",
        CapName = "Mario"
    };

    public string Capture { get; } = "";
    public bool Is2d { get; } = false;
    public byte Scenario { get; } = 0;
    public string Stage { get; } = "";
    public GameMode CurrentGameMode { get; } = GameMode.None;
    public bool IsIt { get; } = false;
    public Time Time { get; } = new(0, 0, DateTime.Now);
    public bool IsSaveLoaded => true;
    public bool IsBanned => false;
    public PlayerAction LastPlayerAction { get; set; }

    public void Disconnect()
    {
        Dispose();
    }

    public void Crash(bool ban)
    {
        Dispose();
    }

    public Task SendShine(int id)
    {
        return Task.CompletedTask;
    }

    public Task ChangeStage(string stage, string warp, sbyte scenario = 0, byte subScenarioType = 0, int delay = 0)
    {
        return Task.CompletedTask;
    }

    public Task Send<T>(T packet, Guid? sender) where T : struct, IPacket
    {
        return Task.CompletedTask;
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
        var comp = _objectController.CreateObject<T>()!;
        comp.Player = this;
        _components[typeof(T)] = comp;
        return comp;
    }

    public async Task Init()
    {
        await BroadcastPacket(new ConnectPacket
        {
            ClientName = Name,
            ConnectionType = ConnectPacket.ConnectionTypes.FirstConnection,
            MaxPlayers = 0
        });
    }

    public async Task BroadcastPacket(IPacket packet)
    {
        var memory = MemoryPool<byte>.Shared.RentZero(Constants.HeaderSize + packet.Size);
        var header = new PacketHeader
        {
            Id = Id,
            Type = _packetManager.GetPacketId(packet.GetType()),
            PacketSize = packet.Size
        };
        PacketHelper.FillPacket(header, packet, memory.Memory);
        var args = new DummySendPacketEventArgs(this, header, packet);
        _eventManager.OnDummySendPacket.RaiseEvent(args);
        if (args.Broadcast)
            await _server.ReplaceBroadcast(args.ReplacePacket ?? args.Packet, Id, args.SpecificReplacePackets);
        memory.Dispose();
    }
}