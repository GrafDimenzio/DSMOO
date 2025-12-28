using DSMOOFramework.Commands;
using DSMOOServer.API.Player;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

#if DEBUG
[Command(
    CommandName = "test",
    Aliases = [],
    Description = "Debug Command",
    Parameters = ["DEBUG"]
)]
public class Debug(PlayerManager manager, DummyManager dummyManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        var dummy = dummyManager.CreateDummy("ActTest").GetAwaiter().GetResult();
        var player = manager.RealPlayers.First();
        dummy.BroadcastPacket(new GamePacket()
        {
            Is2d = false,
            Stage = player.Stage,
            ScenarioNum = player.Scenario
        });
        dummy.BroadcastPacket(new PlayerPacket
        {
            Position = player.Position,
            Rotation = player.Rotation,
            Act = 235
        });
        return "TEST";
    }
}
#endif