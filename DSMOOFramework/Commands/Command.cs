namespace DSMOOFramework.Commands;

public abstract class Command : ICommand
{
    public CommandAttribute CommandInfo { get; set; }

    public virtual CommandResult PreExecute(string command, string[] args, ICommandSender sender)
    {
        if (string.IsNullOrWhiteSpace(CommandInfo.Permission) || sender.HasPermission(CommandInfo.Permission))
            return new CommandResult
            {
                ResultType = ResultType.Success
            };

        return new CommandResult
        {
            Message = "You have insufficient permissions for this command.",
            ResultType = ResultType.NoPermission
        };
    }

    public abstract CommandResult Execute(string command, string[] args, ICommandSender sender);
}