using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

[Analyze(Priority = 0)]
public class ScenarioMergingManager(EventManager eventManager, Server server, ConfigHolder<ServerMainConfig> configHolder) : Manager
{
    private ServerMainConfig Config { get; } = configHolder.Config;
    
    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        switch (args.Packet)
        {
            case GamePacket gamePacket:
                if (Config.ScenarioMerging)
                {
                    foreach (var client in server.Clients)
                    {
                        if (client == args.Sender) continue;
                        if (client.Player.Stage != gamePacket.Stage) continue;
                        //255 means the player is in a transition between worlds
                        if (gamePacket.ScenarioNum == 255) continue;
                        if (client.Player.Scenario == gamePacket.ScenarioNum) continue;
                        var newPacket = new GamePacket()
                        {
                            Is2d = gamePacket.Is2d,
                            Stage = gamePacket.Stage,
                            ScenarioNum = client.Player.Scenario,
                        };
                        args.SpecificReplacePackets[client.Id] = newPacket;
                    }
                }
                break;
        }
    }
}