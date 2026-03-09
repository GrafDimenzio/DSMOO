using DSMOOFramework.Controller;
using DSMOOServer.API.Player;
using DSMOOServer.API.Stage;
using DSMOOServer.API.Stage.Map;

namespace DSMOOServer.API.GameModes.Hints;

public abstract class MapLocationHint : IHint
{
    [Inject] public StageManager StageManager;

    public string GetHint(IPlayer player)
    {
        var location = StageManager.GetMapLocation(player.Position, player.Stage);
        return location == null ? "No location found" : GetHint(player, location);
    }

    protected abstract string GetHint(IPlayer player, MapLocation location);
}

[Hint(Name = "MapNorthSouth")]
public class NorthSouthLocationHint : MapLocationHint
{
    protected override string GetHint(IPlayer player, MapLocation location)
    {
        return location.Y <= 1024 ? "North" : "South";
    }
}

[Hint(Name = "MapEastWest")]
public class EastWestLocationHint : MapLocationHint
{
    protected override string GetHint(IPlayer player, MapLocation location)
    {
        return location.X <= 1024 ? "West" : "East";
    }
}

[Hint(Name = "MapNumber")]
public class NumberLocationHint : MapLocationHint
{
    protected override string GetHint(IPlayer player, MapLocation location)
    {
        return location.Number.ToString();
    }
}

[Hint(Name = "MapLetter")]
public class LetterLocationHint : MapLocationHint
{
    protected override string GetHint(IPlayer player, MapLocation location)
    {
        return location.GetLetter();
    }
}

[Hint(Name = "MapCell")]
public class CellLocationHint : MapLocationHint
{
    protected override string GetHint(IPlayer player, MapLocation location)
    {
        return location.GetCell();
    }
}