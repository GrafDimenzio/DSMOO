using DSMOOFramework.Commands;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;
using DSMOOPlus.Commands;
using DSMOOPlus.Enum;
using DSMOOPlus.Packets;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Network.Packets;
using InitPacket = DSMOOServer.Network.Packets.InitPacket;

namespace DSMOOPlus;

[Plugin(
    Author = "Dimenzio",
    Name = "DSMOOPlus",
    Description = "Adds support for SMOO+",
    Repository = "https://github.com/GrafDimenzio/DSMOO",
    Version = "0.0.3"
)]
public class DSMOOPlusManager(EventManager eventManager, CommandManager commandManager, ILogger logger) : Manager
{
    public override void Initialize()
    {
        eventManager.OnSendPlayerInitPacket.Subscribe(OnSendInitPacket);
        eventManager.OnPlayerAddComponents.Subscribe(OnAddComponent);
        eventManager.OnPacketReceived.Subscribe(OnPacket);
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        switch (args.Packet)
        {
            case ChangeCostumePacket changeCostumePacket:
            case PlayerStatePacket playerStatePacket:
                logger.Warn("Received packet  " + args.Packet.GetType().Name);
                break;

            case DisconnectPacket:
                args.Sender.Send(new UnhandledPacket());
                break;
            
            case MessagePacket sendMessagePacket:
                break;
                switch (sendMessagePacket.MessageType)
                {
                    //TODO: Only do this for SYSTEM Messages. For Testing every type will react
                    case MessageType.Chat:
                    case MessageType.Private:
                    case MessageType.System:
                        var response = commandManager.ProcessQuery(sendMessagePacket.Message,
                            args.Sender.Player.GetComponent<PlayerPlus>()!.CommandSender);
                        args.Sender.Player.GetComponent<PlayerPlus>()!
                            .SendMessage($"[Server] [{response.ResultType}] {response.Message}");
                        break;
                }
                break;
        }
    }

    private void OnAddComponent(PlayerAddComponentsEventArgs args)
    {
        var comp = args.Player.AddComponent<PlayerPlus>();
        comp.CommandSender = new PlayerCommandSender(args.Player);
    }

    private void OnSendInitPacket(SendPlayerInitPacketEventArgs args)
    {
        if (args.Packet is not InitPacket original) return;
        args.Packet = new Packets.InitPacket
        {
            MaxPlayers = original.MaxPlayers
        };
    }
}