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

    private ushort _act = 500; //This means the dummy will be standing

    private float[] _animationBlendWeights = [];

    private string _capture = "";

    private CostumePacket _costume = new() { BodyName = "Mario", CapName = "Mario" };

    private GameMode _gameMode = GameMode.None;

    private bool _is2D;

    private bool _isIt;

    private string _name = "dummy";

    private Vector3 _position = Vector3.Zero;

    private Quaternion _rotation = Quaternion.Identity;

    private byte _scenario;

    private string _stage = "";

    private ushort _subAct;

    private Time _time = new(0, 0, DateTime.Now);


    public Dummy(string name, Server server, PlayerManager playerManager, EventManager eventManager, JoinManager joinManager,
        PacketManager packetManager, ObjectController objectController)
    {
        _name = name;
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
        BroadcastPacketAsync(new DisconnectPacket()).GetAwaiter().GetResult();
        _playerManager.Players.Remove(this);
    }

    public IPAddress? Ip { get; } = null;
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsDummy => true;

    public string Name
    {
        get => _name;
        set => Task.Run(async () =>
        {
            await BroadcastPacketAsync(new DisconnectPacket());
            await Task.Delay(100);
            await BroadcastPacketAsync(new ConnectPacket
            {
                ConnectionType = ConnectPacket.ConnectionTypes.Reconnecting,
                ClientName = value
            });
            await BroadcastStateAsync();
        }).GetAwaiter().GetResult();
    }

    public Vector3 Position
    {
        get => _position;
        set =>
            BroadcastPacket(new PlayerPacket
            {
                Position = value,
                Act = Act,
                Rotation = Rotation,
                SubAct = SubAct,
                AnimationBlendWeights = AnimationBlendWeights
            }, true);
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set =>
            BroadcastPacket(new PlayerPacket
            {
                Position = Position,
                Act = Act,
                Rotation = value,
                SubAct = SubAct,
                AnimationBlendWeights = AnimationBlendWeights
            }, true);
    }

    public ushort Act
    {
        get => _act;
        set =>
            BroadcastPacket(new PlayerPacket
            {
                Position = Position,
                Act = value,
                Rotation = Rotation,
                SubAct = SubAct,
                AnimationBlendWeights = AnimationBlendWeights
            }, true);
    }

    public ushort SubAct
    {
        get => _subAct;
        set =>
            BroadcastPacket(new PlayerPacket
            {
                Position = Position,
                Act = Act,
                Rotation = Rotation,
                SubAct = value,
                AnimationBlendWeights = AnimationBlendWeights
            }, true);
    }

    public float[] AnimationBlendWeights
    {
        get => _animationBlendWeights;
        set =>
            BroadcastPacket(new PlayerPacket
            {
                Position = Position,
                Act = Act,
                Rotation = Rotation,
                SubAct = SubAct,
                AnimationBlendWeights = value
            }, true);
    }

    public CostumePacket Costume
    {
        get => _costume;
        set => BroadcastPacket(value, true);
    }

    public string Capture
    {
        get => _capture;
        set => BroadcastPacket(new CapturePacket { ModelName = value }, true);
    }

    public string Stage
    {
        get => _stage;
        set =>
            BroadcastPacket(new GamePacket
            {
                Stage = value,
                Is2d = Is2d,
                ScenarioNum = Scenario
            }, true);
    }

    public bool Is2d
    {
        get => _is2D;
        set =>
            BroadcastPacket(new GamePacket
            {
                Stage = Stage,
                Is2d = value,
                ScenarioNum = Scenario
            }, true);
    }

    public byte Scenario
    {
        get => _scenario;
        set =>
            BroadcastPacket(new GamePacket
            {
                Stage = Stage,
                ScenarioNum = value,
                Is2d = Is2d
            }, true);
    }

    public GameMode CurrentGameMode
    {
        get => _gameMode;
        set => BroadcastPacket(new TagPacket
        {
            GameMode = value,
            UpdateType = TagPacket.TagUpdate.None
        }, true);
    }

    public bool IsIt
    {
        get => _isIt;
        set => BroadcastPacket(new TagPacket
        {
            GameMode = CurrentGameMode,
            IsIt = value,
            UpdateType = TagPacket.TagUpdate.State
        }, true);
    }

    public Time Time
    {
        get => _time;
        set => BroadcastPacket(new TagPacket
        {
            GameMode = CurrentGameMode,
            Seconds = value.Seconds,
            Minutes = value.Minutes,
            UpdateType = TagPacket.TagUpdate.Time
        }, true);
    }

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
        //TODO: Set Position based on Warp
        return BroadcastPacketAsync(new GamePacket
        {
            Stage = stage,
            Is2d = Is2d,
            ScenarioNum = scenario >= 0 ? (byte)scenario : (byte)0
        });
    }

    public Task Send<T>(T packet, Guid? sender) where T : struct, IPacket
    {
        //Send is to display things client side but a dummy doesn't have a client side
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
        await BroadcastPacketAsync(new ConnectPacket
        {
            ClientName = Name,
            ConnectionType = ConnectPacket.ConnectionTypes.FirstConnection,
            MaxPlayers = 0
        });
    }

    public void BroadcastPacket(IPacket packet, bool await = false)
    {
        if (await)
            BroadcastPacketAsync(packet).GetAwaiter().GetResult();
        else
            Task.Run(() => BroadcastPacketAsync(packet));
    }

    public async Task BroadcastPacketAsync(IPacket packet)
    {
        UpdateValues(packet);
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

    public void BroadcastState(bool await = false)
    {
        if (await)
            BroadcastStateAsync().GetAwaiter().GetResult();
        else
            Task.Run(BroadcastStateAsync);
    }

    public async Task BroadcastStateAsync()
    {
        await BroadcastPacketAsync(new CostumePacket()
        {
            CapName = _costume.CapName,
            BodyName = _costume.BodyName,
        });
        await BroadcastPacketAsync(new CapturePacket()
        {
            ModelName = _capture
        });
        await BroadcastPacketAsync(new TagPacket
        {
            UpdateType = TagPacket.TagUpdate.Both,
            GameMode = _gameMode,
            IsIt = _isIt,
            Minutes = _time.Minutes,
            Seconds = _time.Seconds,
        });
        await BroadcastPacketAsync(new GamePacket()
        {
            Is2d = _is2D,
            Stage = _stage,
            ScenarioNum = _scenario,
        });
        await BroadcastPacketAsync(new PlayerPacket()
        {
            Position = _position,
            Rotation = _rotation,
            Act = _act,
            SubAct = _subAct,
            AnimationBlendWeights = _animationBlendWeights,
        });
    }

    private void UpdateValues(IPacket packet)
    {
        switch (packet)
        {
            case ConnectPacket connectPacket:
                _name = connectPacket.ClientName;
                break;

            case PlayerPacket playerPacket:
                _position = playerPacket.Position;
                _rotation = playerPacket.Rotation;
                _act = playerPacket.Act;
                _subAct = playerPacket.SubAct;
                _animationBlendWeights = playerPacket.AnimationBlendWeights;
                break;

            case CostumePacket costumePacket:
                _costume = costumePacket;
                break;

            case CapturePacket capturePacket:
                _capture = capturePacket.ModelName;
                break;

            case GamePacket gamePacket:
                _stage = gamePacket.Stage;
                _is2D = gamePacket.Is2d;
                _scenario = gamePacket.ScenarioNum;
                break;

            case TagPacket tagPacket:
                _gameMode = tagPacket.GameMode;
                if (tagPacket.UpdateType.HasFlag(TagPacket.TagUpdate.Time))
                    _time = (tagPacket.Seconds, tagPacket.Minutes);
                if (tagPacket.UpdateType.HasFlag(TagPacket.TagUpdate.State))
                    _isIt = tagPacket.IsIt;
                break;
        }
    }
}