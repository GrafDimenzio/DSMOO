using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

public interface IHint
{
    public string GetHint(IPlayer player);
}