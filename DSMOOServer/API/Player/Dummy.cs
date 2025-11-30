using System.Buffers;
using System.Net;
using System.Numerics;
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
    private readonly Server _server;
    private readonly PlayerManager _playerManager;
    private readonly EventManager _eventManager;
    private readonly JoinManager _joinManager;

    public Dummy(Server server, PlayerManager playerManager, EventManager eventManager, JoinManager joinManager)
    {
        _server = server;
        _playerManager = playerManager;
        _eventManager = eventManager;
        _joinManager = joinManager;
        _playerManager.Players.Add(this);
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

    public CostumePacket Costume { get; } = new CostumePacket()
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
    public Time Time { get; } = new Time(0, 0, DateTime.Now);
    public bool IsSaveLoaded => true;
    public bool IsBanned => false;

    public void Disconnect() => Dispose();

    public void Crash(bool ban) => Dispose();

    public void SendShine(int id) { }

    public void ChangeStage(string stage, string warp, sbyte scenario = 0, byte subScenarioType = 0)
    {
        
    }

    public Task Send<T>(T packet, Guid? sender) where T : struct, IPacket => Task.CompletedTask;

    public async Task Init()
    {
        await BroadcastPacket(new ConnectPacket()
        {
            ClientName = Name,
            ConnectionType = ConnectPacket.ConnectionTypes.FirstConnection,
            MaxPlayers = 0
        });
    }

    public async Task BroadcastPacket(IPacket packet)
    {
        var memory = MemoryPool<byte>.Shared.RentZero(Constants.HeaderSize + packet.Size);
        var header = new PacketHeader {
            Id         = Id,
            Type       = Constants.PacketMap[packet.GetType()].Type,
            PacketSize = packet.Size,
        };
        PacketHelper.FillPacket(header, packet, memory.Memory);
        var args = new DummySendPacketEventArgs(this, header, packet);
        _eventManager.OnDummySendPacket.RaiseEvent(args);
        if (args.Broadcast)
            await _server.ReplaceBroadcast(args.ReplacePacket ?? args.Packet, Id, args.SpecificReplacePackets);
        memory.Dispose();
    }

    public void Dispose()
    {
        BroadcastPacket(new DisconnectPacket()).GetAwaiter().GetResult();
        _playerManager.Players.Remove(this);
    }
}