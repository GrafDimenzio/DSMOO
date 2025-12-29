using DSMOOFramework.Config;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

public class GameModeManager(
    EventManager eventManager,
    ConfigHolder<ServerMainConfig> configHolder,
    PlayerManager playerManager) : Manager
{
    public override void Initialize()
    {
        eventManager.OnPlayerState.Subscribe(OnPlayerState);
    }

    private void OnPlayerState(PlayerStateEventArgs args)
    {
        if (configHolder.Config.HidersCanSeeEachOther || args.Player.IsIt ||
            args.Player.CurrentGameMode == GameMode.None) return;

        foreach (var realPlayer in playerManager.RealPlayers)
            if (!realPlayer.IsIt && realPlayer.CurrentGameMode != GameMode.None)
                args.SpecificInvisible.Add(realPlayer.Id);
    }
}