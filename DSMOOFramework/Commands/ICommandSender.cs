namespace DSMOOFramework.Commands;

public interface ICommandSender
{
    public string Name { get; }

    public bool HasPermission(string permission);
}