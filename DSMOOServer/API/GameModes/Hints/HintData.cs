using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

public struct HintData
{
    public string HintName { get; set; }
    public string HintText { get; set; }
    public IPlayer Player { get; set; }
}