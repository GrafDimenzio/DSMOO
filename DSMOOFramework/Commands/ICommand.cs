namespace DSMOOFramework.Commands;

public interface ICommand
{
    public CommandAttribute CommandInfo { get; set; }

    public CommandResult PreExecute(string command, string[] args);

    public CommandResult Execute(string command, string[] args);
}