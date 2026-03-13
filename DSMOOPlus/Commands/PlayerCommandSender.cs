using DSMOOFramework.Commands;
using DSMOOServer.API.Player;

namespace DSMOOPlus.Commands;

public class PlayerCommandSender(IPlayer player) : ICommandSender
{
    public IPlayer Player => player;
    
    public string Name => player.Name;
    public bool HasPermission(string permission)
    {
        return true;
    }
}