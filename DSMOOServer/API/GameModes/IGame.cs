using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public interface IGame
{
    public bool IsRunning { get; }
    public string DisplayName { get; }
    public IPlayer[] Players { get; }
    public StageConfig StageConfig { get; }
    public HintConfig HintConfig { get; }
    public void StartGame(IPlayer[] playingPlayers, StageConfig stageConfig, HintConfig hintConfig, string[] arguments);
    public void EndGame();
}