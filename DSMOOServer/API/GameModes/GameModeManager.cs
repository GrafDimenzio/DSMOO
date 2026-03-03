using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.GameModes.Hints;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

public class GameModeManager(
    EventManager eventManager,
    ConfigHolder<ServerMainConfig> configHolder,
    Analyzer analyzer,
    ObjectController objectController,
    ILogger logger,
    PlayerManager playerManager) : Manager
{
    public Dictionary<string, IHint> Hints = new();
    
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
        eventManager.OnPlayerState.Subscribe(OnPlayerState);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<IHint>()) return;
        var name = args.GetAttribute<HintAttribute>()?.Name;
        if (name == null) return;
        var hintObject = objectController.GetObject(args.Type);
        if (hintObject is not IHint hint) return;
        Hints[name] = hint;
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