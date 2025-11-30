namespace DSMOOFramework.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    public string CommandName { get; set; } = "";
    public string[] Aliases { get; set; } = [];
    public string[] Parameters { get; set; } = [];
    public string Description { get; set; } = "";
}