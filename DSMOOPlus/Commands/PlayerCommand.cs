using DSMOOFramework.Commands;
using DSMOOServer.API.Player;

namespace DSMOOPlus.Commands;

public abstract class PlayerCommand : Command
{
    public sealed override CommandResult Execute(string command, string[] args, ICommandSender sender)
    {
        if (sender is not PlayerCommandSender playerSender)
            return new CommandResult
            {
                ResultType = ResultType.Error,
                Message = "This command can only be executed by a Player!"
            };
        
        return Execute(command, args, playerSender.Player);
    }

    protected abstract CommandResult Execute(string command, string[] args, IPlayer player);
}