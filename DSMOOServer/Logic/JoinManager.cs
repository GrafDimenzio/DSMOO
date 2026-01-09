using DSMOOFramework.Config;
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

        args.Sender.FirstPacketSend = true;

        args.Sender.Id = args.Header.Id;
        args.Sender.Name = connectPacket.ClientName;

        var initArgs = new SendPlayerInitPacketEventArgs(new InitPacket
        {
            MaxPlayers = Config.MaxPlayers
        }, args.Sender.Player);
        eventManager.OnSendPlayerInitPacket.RaiseEvent(initArgs);
        args.Sender.Send(initArgs.Packet).GetAwaiter().GetResult();

        var preJoinArgs = new PlayerPreJoinEventArgs
        {
            Client = args.Sender,
            AllowJoin = playerManager.PlayerCount <= Config.MaxPlayers
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

        lock (server.Clients)
        {
            if (!Enum.IsDefined(connectPacket.ConnectionType))
                throw new Exception(
                    $"Invalid connection type {connectPacket.ConnectionType} for {args.Sender.Name} ({args.Sender.Id}/{args.Sender.Socket.RemoteEndPoint})");

            var oldClient = server.Clients.Find(x => x.Id == args.Sender.Id);
            if (oldClient != null)
            {
                Logger.Info($"DETECTED OLD CLIENT ID {oldClient.Id}");
                if (connectPacket.ConnectionType == ConnectPacket.ConnectionTypes.Reconnecting)
                    args.Sender.Player.CopyDataFromOtherPlayer(oldClient.Player);
                else
                    isRestartReconnect = true;
                server.Clients.Remove(oldClient);
                if (oldClient.Socket.Connected)
                {
                    oldClient.Logger.Info(
                        $"Disconnecting already connected client {oldClient.Socket?.RemoteEndPoint} for {args.Sender.Socket?.RemoteEndPoint}");
                    oldClient.Dispose();
                }
            }

            server.Clients.Add(args.Sender);
        }

        Parallel.ForEachAsync(playerManager.Players.FindAll(x => x.Id != args.Sender.Id),
            async (ply, _) => { await SendPlayerStateToOtherPlayer(ply, args.Sender.Player); });
        if (isRestartReconnect)
            Parallel.ForEachAsync(playerManager.Players.FindAll(x => x.Id != args.Sender.Id),
                async (ply, _) => { await SendPlayerStateToOtherPlayer(args.Sender.Player, ply); });


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
}