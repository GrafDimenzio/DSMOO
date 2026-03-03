using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

[Hint(Name = "Position")]
public class PositionHint : IHint
{
    public string GetHint(IPlayer player)
    {
        return $"{player.Position.X},{player.Position.Y},{player.Position.Z}";
    }
}