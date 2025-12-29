using DSMOOFramework.Commands;
using DSMOOPlus.Packets;
using DSMOOServer.Logic;

namespace DSMOOPlus.Commands;

[Command(
    CommandName = "plus",
    Aliases = [],
    Description = "",
    Parameters = []
)]
public class TestCommand(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        foreach (var player in manager.RealPlayers)
        {
            player.Send(new PlayerStatePacket
            {
                Coins = 500,
                Health = 1,
                AssistModeHealth = false
            }, null);
        }
        return "Executed 5";
    }
}