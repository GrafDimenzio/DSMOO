using DSMOOFramework.Config;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

[Hint(Name = "Area")]
public class AreaHint(ConfigHolder<GameModeConfig> configHolder) : IHint
{
    public string GetHint(IPlayer player)
    {
        if (configHolder.Config.AreaTypes.TryGetValue(player.Stage, out var type))
            return type;
        return "Sub-Area";
    }
}