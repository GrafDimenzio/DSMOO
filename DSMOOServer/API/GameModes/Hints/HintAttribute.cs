namespace DSMOOServer.API.GameModes.Hints;

[AttributeUsage(AttributeTargets.Class)]
public class HintAttribute : Attribute
{
    public string Name { get; set; }
}