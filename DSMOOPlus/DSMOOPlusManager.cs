using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;
using DSMOOPlus.Packets;
using DSMOOServer.API.Enum;
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
    Version = "1.0.0"
)]
public class DSMOOPlusManager(EventManager eventManager, ILogger logger) : Manager
{
    public override void Initialize()
    {
        eventManager.OnSendPlayerInitPacket.Subscribe(OnSendInitPacket);
        eventManager.OnPlayerAddComponents.Subscribe(OnAddComponent);
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        eventManager.OnPlayerAction.Subscribe(OnPlayerAction);
    }

    private void OnPlayerAction(PlayerActionEventArgs args)
    {
        if (args.Action != PlayerAction.None)
            args.Player.GetComponent<PlayerPlus>()!.SendMessage(args.Action.ToString());
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        switch (args.Packet)
        {
            case ChangeCostumePacket changeCostumePacket:
            case MessagePacket sendMessagePacket:
            case PlayerStatePacket playerStatePacket:
                logger.Warn("Received packet  " + args.Packet.GetType().Name);
                break;

            case DisconnectPacket:
                args.Sender.Send(new UnhandledPacket());
                break;
        }
    }

    private void OnAddComponent(PlayerAddComponentsEventArgs args)
    {
        args.Player.AddComponent<PlayerPlus>();
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