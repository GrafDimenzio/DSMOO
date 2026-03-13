using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

[Hint(Name = "StageName")]
public class StageNameHint : IHint
{
    public string GetHint(IPlayer player)
    {
        return player.Stage;
    }
}