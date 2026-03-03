using DSMOOFramework.Analyzer;
using DSMOOServer.API.Map;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

public abstract class MapLocationHint : IHint
{
    public string GetHint(IPlayer player)
    {
        return GetHint(player, new MapLocation(player.Position, player.Stage));
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