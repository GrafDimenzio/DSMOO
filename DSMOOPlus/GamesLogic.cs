using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Logic;

namespace DSMOOPlus;

public class GamesLogic(EventManager eventManager, PlayerManager playerManager, ILogger logger) : Manager
{
    public override void Initialize()
    {
        eventManager.OnSendHint.Subscribe(OnSendHint);
        eventManager.OnWaitingPlayersReleased.Subscribe(OnWaitingPlayersReleased);
        eventManager.OnGameStart.Subscribe(OnGameStart);
        eventManager.OnGameEnd.Subscribe(OnGameEnd);
    }
    
    private void OnWaitingPlayersReleased(WaitingGameEventArgs gameEventArgs)
    {
        foreach (var player in playerManager.RealPlayers)
        {
            player.GetComponent<PlayerPlus>().SendMessage($"Waiting Players have been released");
        }
    }
    
    private void OnGameEnd(GameEventArgs gameEventArgs)
    {
        foreach (var player in playerManager.RealPlayers)
        {
            player.GetComponent<PlayerPlus>().SendMessage($"{gameEventArgs.Game.DisplayName} round ended");
        }
    }
    
    private void OnGameStart(GameEventArgs gameEventArgs)
    {
        foreach (var player in playerManager.RealPlayers)
        {
            player.GetComponent<PlayerPlus>().SendMessage($"Started a round of {gameEventArgs.Game.DisplayName}");
        }
    }

    private void OnSendHint(SendHintEventArgs ev)
    {
        ev.SendToPlayer.GetComponent<PlayerPlus>()
            .SendMessage($"Hint {ev.Hint.Player.Name}: {string.Join(' ', ev.Hint.HintTexts)}");
    }
}