using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

public class JoinManager(
    EventManager eventManager,
    Server server,
    ILogger logger,
    PlayerManager playerManager,
    ObjectController objectController,
    ConfigHolder<ServerMainConfig> configHolder) : Manager
{
    private ILogger Logger { get; } = logger;

    private ServerMainConfig Config { get; } = configHolder.Config;

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacketReceived);
    }

    private void OnPacketReceived(PacketReceivedEventArgs args)
    {
        if (args.Packet is not ConnectPacket connectPacket)
        {
            if (!args.Sender.FirstPacketSend)
                Logger.Debug(
                    $"First packet was not init, instead it was {args.PacketType} ({args.Sender.Socket.RemoteEndPoint})");
            return;
        }

        if (args.Sender.FirstPacketSend)
            return;

        args.Sender.FirstPacketSend = true;

        args.Sender.Id = args.Header.Id;
        args.Sender.Name = connectPacket.ClientName;

        var initArgs = new SendPlayerInitPacketEventArgs(new InitPacket
        {
            MaxPlayers = Config.MaxPlayers
        }, args.Sender);
        eventManager.OnSendPlayerInitPacket.RaiseEvent(initArgs);
        args.Sender.Send(initArgs.Packet).GetAwaiter().GetResult();

        var preJoinArgs = new PlayerPreJoinEventArgs
        {
            Client = args.Sender,
            AllowJoin = CanJoin(args.Sender.Id)
        };
        if (!preJoinArgs.AllowJoin)
            args.Sender.Logger.Warn("Rejected join since the server reached max players amount");
        eventManager.OnPlayerPreJoin.RaiseEvent(preJoinArgs);

        if (!preJoinArgs.AllowJoin)
        {
            args.Sender.Ignored = true;
            return;
        }

        var isRestartReconnect = false;

        if (!Enum.IsDefined(connectPacket.ConnectionType))
            throw new Exception(
                $"Invalid connection type {connectPacket.ConnectionType} for {args.Sender.Name} ({args.Sender.Id}/{args.Sender.Socket.RemoteEndPoint})");

        if (!server.Clients.TryGetValue(args.Sender.Id, out var oldClient))
        {
            CreatePlayerForClient(args.Sender);
        }
        else
        {
            Logger.Info($"DETECTED OLD CLIENT ID {oldClient.Id}");
            if (connectPacket.ConnectionType == ConnectPacket.ConnectionTypes.Reconnecting)
            {
                //Migrate Player Object to new Client
                Logger.Debug("MIGRATE PLAYER OBJECT");
                args.Sender.Player = oldClient.Player;
                oldClient.GotMigrated = true;
                args.Sender.Player!.Client = args.Sender;
            }
            else
            {
                isRestartReconnect = true;
                CreatePlayerForClient(args.Sender);
            }

            oldClient.Logger.Info(
                $"Disconnecting already connected client with id {oldClient.Id}");
            server.Clients.TryRemove(oldClient.Id, out _);
            try
            {
                oldClient.Socket.Disconnect(false);
            }
            catch (ObjectDisposedException)
            {
                //Ignore when already disposed - will be disconnected automatically soon
            }
        }

        server.Clients.TryAdd(args.Sender.Id, args.Sender);

        foreach (var ply in playerManager.Players)
        {
            if (ply.Id == args.Sender.Id)
                continue;
            _ = SendPlayerStateToOtherPlayer(ply, args.Sender.Player);

            //A Player that completely restarts but was still connected no longer has the same values like Stage
            //So the default Values will be send to update all Players
            if (isRestartReconnect)
                _ = SendPlayerStateToOtherPlayer(args.Sender.Player, ply);
        }


        eventManager.OnPlayerJoined.RaiseEvent(new PlayerJoinedEventArgs
        {
            Player = args.Sender.Player
        });

        Logger.Info($"Client {args.Sender.Name} ({args.Sender.Id}/{args.Sender.Socket.RemoteEndPoint}) connected.");
    }

    public async Task SendPlayerStateToOtherPlayer(IPlayer ply, IPlayer receiver)
    {
        await receiver.Send(new ConnectPacket
        {
            ConnectionType = ConnectPacket.ConnectionTypes.FirstConnection,
            MaxPlayers = Config.MaxPlayers,
            ClientName = ply.Name
        }, ply.Id);
        await receiver.Send(new CostumePacket
        {
            CapName = ply.Costume.CapName,
            BodyName = ply.Costume.BodyName
        }, ply.Id);
        await receiver.Send(new CapturePacket
        {
            ModelName = ply.Capture
        }, ply.Id);
        await receiver.Send(new TagPacket
        {
            UpdateType = TagPacket.TagUpdate.Both,
            GameMode = ply.CurrentGameMode,
            IsIt = ply.IsIt,
            Minutes = ply.Time.Minutes,
            Seconds = ply.Time.Seconds
        }, ply.Id);
        await receiver.Send(new GamePacket
        {
            Is2d = ply.Is2d,
            Stage = ply.Stage,
            ScenarioNum = ply.Scenario
        }, ply.Id);
        await receiver.Send(new PlayerPacket
        {
            Position = ply.Position,
            Rotation = ply.Rotation,
            Act = ply.Act,
            SubAct = ply.SubAct,
            AnimationBlendWeights = ply.AnimationBlendWeights
        }, ply.Id);
    }

    private void CreatePlayerForClient(Client client)
    {
        client.Player = new Player(client, objectController);
        eventManager.OnPlayerAddComponents.RaiseEvent(new PlayerAddComponentsEventArgs { Player = client.Player });
        lock (playerManager.PlayerList)
        {
            playerManager.PlayerList.Add(client.Player);
        }
    }

    private bool CanJoin(Guid guid)
    {
        var ids = playerManager.PlayerList.Where(x => !x.IsDummy).Select(x => x.Id).ToList();
        ids.Add(guid);
        ids = ids.Distinct().ToList();
        return ids.Count <= Config.MaxPlayers;
    }
}