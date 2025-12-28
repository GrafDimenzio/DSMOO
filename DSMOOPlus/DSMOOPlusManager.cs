using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Network.Packets;

namespace DSMOOPlus;

[Plugin(
    Author = "Dimenzio",
    Name = "DSMOOPlus",
    Description = "Adds support for SMOO+",
    Repository = "https://github.com/GrafDimenzio/DSMOO",
    Version = "1.0.0"
    )]
public class DSMOOPlusManager(EventManager eventManager) : Manager
{
    public override void Initialize()
    {
        eventManager.OnSendPlayerInitPacket.Subscribe(OnSendInitPacket);
    }

    private void OnSendInitPacket(SendPlayerInitPacketEventArgs args)
    {
        if (args.Packet is not InitPacket original) return;
        args.Packet = new Packets.InitPacket()
        {
            MaxPlayers = original.MaxPlayers,
        };
    }
}