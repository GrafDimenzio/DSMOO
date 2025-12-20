namespace DSMOOWebInterface.Setup.Controller;

public class ControllerAttribute : Attribute
{
    public required string Route { get; init; }
    public required ControllerType ControllerType { get; init; }
}