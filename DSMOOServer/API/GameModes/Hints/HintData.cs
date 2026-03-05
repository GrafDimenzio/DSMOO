using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes.Hints;

public struct HintData
{
    public string[] HintTypes { get; set; }
    public string[] HintTexts { get; set; }
    public IPlayer Player { get; set; }
}