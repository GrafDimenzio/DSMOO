using System.Numerics;
using DSMOOFramework.Commands;
using DSMOOFramework.Logger;
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
public class Debug(PlayerManager manager, DummyManager dummyManager, ILogger logger) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        Task.Run(Run);
        return "TEST";
    }

    public async Task Run()
    {
        var player = manager.Players[0];
        var dummy = await dummyManager.CreateDummy("FirstName");
        dummy.Costume = player.Costume;
        dummy.Stage = player.Stage;
        dummy.Position = player.Position;
        await Task.Delay(1000);
        dummy.Name = "NewName";
        await Task.Delay(4000);
        dummy.Rotation = player.Rotation;
        var forward = Vector3.Transform(Vector3.UnitZ, dummy.Rotation) * 30;
        logger.Warn(forward);
        for (var i = 0; i < 300; i++)
        {
            await Task.Delay(50);
            dummy.Position += forward;
        }
        dummy.Disconnect();
    }
}
#endif