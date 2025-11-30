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
    public override CommandResult Execute(string command, string[] args)
    {
        return manager.Players.Count == 0
            ? "No Players connected"
            : "Players: " + string.Join(", ", manager.Players.Select(p => p.Name));
    }
}