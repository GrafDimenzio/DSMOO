using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public interface IGame
{
    public string Name { get; }
    public IPlayer[] Players { get; }
    public StageConfig StageConfig { get; }
    public HintConfig HintConfig { get; }
    public void StartGame(IPlayer[] playingPlayers, StageConfig stageConfig, HintConfig hintConfig, string[] arguments);
    public void EndGame();
}