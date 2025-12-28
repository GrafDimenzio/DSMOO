namespace DSMOOWebInterface.Models;

public class NavigationElementModel
{
    public required string Href { get; set; }
    public required string Svg { get; set; }
    public required string Text { get; set; }
    public int SelectionId { get; set; }
    public bool AdminRequired { get; set; }
    public bool PlayerLoginRequired { get; set; }
}