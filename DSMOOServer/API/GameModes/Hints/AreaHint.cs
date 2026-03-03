using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

[Hint(Name = "Area")]
public class AreaHint : IHint
{
    public string GetHint(IPlayer player)
    {
        return "NOT IMPLEMENTED";
    }
}