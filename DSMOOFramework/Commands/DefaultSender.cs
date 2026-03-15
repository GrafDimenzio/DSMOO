namespace DSMOOFramework.Commands;

public class DefaultSender : ICommandSender
{
    public string Name => "DefaultSender";

    public bool HasPermission(string permission)
    {
        return true;
    }
}