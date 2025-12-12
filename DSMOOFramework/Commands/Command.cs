namespace DSMOOFramework.Commands;

public abstract class Command : ICommand
{
    public CommandAttribute CommandInfo { get; set; }

    public virtual CommandResult PreExecute(string command, string[] args)
    {
        return new CommandResult
        {
            ResultType = ResultType.Success
        };
    }

    public abstract CommandResult Execute(string command, string[] args);
}