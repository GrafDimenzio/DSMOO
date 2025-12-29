using DSMOOFramework.Controller;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.Connection;
using DSMOOServer.Logic;
using DSMOOServer.Network;

namespace DSMOOServer.API.Player;

public class DummyManager(
    Server server,
    PlayerManager playerManager,
    EventManager eventManager,
    JoinManager joinManager,
    ObjectController objectController,
    PacketManager packetManager) : Manager
{
    public async Task<Dummy> CreateDummy(string? dummyName = null)
    {
        var dummy = new Dummy(server, playerManager, eventManager, joinManager, packetManager, objectController);
        if (dummyName != null)
            dummy.Name = dummyName;
        await dummy.Init();
        return dummy;
    }
}