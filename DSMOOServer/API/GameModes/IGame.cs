using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public interface IGame
{
    public bool IsRunning { get; }
    public string DisplayName { get; }
    public IPlayer[] Players { get; }
    public StagePreset StagePreset { get; }
    public HintPreset HintPreset { get; }
    public void StartGame(IPlayer[] playingPlayers, StagePreset stagePreset, HintPreset hintPreset, string[] arguments);
    public void EndGame();
    public void AddPlayerToGame(IPlayer player);
    public void RemovePlayerFromGame(IPlayer player);
}