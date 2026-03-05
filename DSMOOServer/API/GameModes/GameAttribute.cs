namespace DSMOOServer.API.GameModes;

public class GameAttribute : Attribute
{
    public required string[] Names { get; init; }
}