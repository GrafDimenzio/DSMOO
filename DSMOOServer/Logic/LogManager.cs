using DSMOOFramework.Analyzer;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Network.Packets;
using EventManager = DSMOOServer.API.Events.EventManager;

namespace DSMOOServer.Logic;

[Analyze(Priority = 0)]
public class LogManager(EventManager eventManager, ILogger logger) : Manager
{
    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        switch (args.Packet)
        {
            case GamePacket gamePacket:
                args.Sender.Logger.Info($"Got game packet {gamePacket.Stage}->{gamePacket.ScenarioNum}");
                break;
            
            case CostumePacket costumePacket:
                args.Sender.Logger.Info($"Got costume packet from {costumePacket.BodyName}->{costumePacket.CapName}");
                break;
        }
    }
}