using DSMOOFramework.Analyzer;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Player;
using DSMOOServer.Connection;

namespace DSMOOServer.Logic;

[Analyze(Priority = 10)]
public class DummyManager(Server server, PlayerManager playerManager, EventManager eventManager, JoinManager joinManager) : Manager
{
    public async Task<Dummy> CreateDummy(string? dummyName = null)
    {
        var dummy = new Dummy(server, playerManager, eventManager, joinManager);
        if (dummyName != null)
            dummy.Name = dummyName;
        await dummy.Init();
        return dummy;
    }
}