using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "list",
    Aliases = ["players"],
    Description = "Shows a list of all Players",
    Parameters = []
)]
public class List(PlayerManager manager) : Command
{
    public override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        if(manager.Players.Count == 0)
            return "No players are currently connected.";

        var msg = new StringBuilder();
        msg.AppendLine("");

        foreach (var player in manager.Players)
        {
            msg.AppendLine($"{player.Name}");
            msg.AppendLine($"   - Stage: {player.Stage}->{player.Scenario}");
            msg.AppendLine($"   - Position: {player.Position}");
        }
        
        return msg.ToString();
        
        return manager.Players.Count == 0
            ? "No Players connected"
            : "Players: " + string.Join(", ", manager.Players.Select(p => p.Name));
    }
}