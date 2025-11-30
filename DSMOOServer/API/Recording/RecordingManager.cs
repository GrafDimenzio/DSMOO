using DSMOOFramework.Analyzer;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Logic;

namespace DSMOOServer.API.Recording;

[Analyze(Priority = 5)]
public class RecordingManager(DummyManager dummyManager, EventManager eventManager) : Manager
{
    public Recording Test = new Recording()
    {
        Elements = [],
    };
    
    public async Task PlayRecording(Recording recording)
    {
        var dummy = await dummyManager.CreateDummy("Replay");

        foreach (var element in recording.Elements)
        {
            await Task.Delay(element.Header.Timestamp);
            await dummy.BroadcastPacket(element.Packet);
        }
        
        dummy.Dispose();
    }

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
    }

    private void OnPacket(PacketReceivedEventArgs e)
    {
        if(!Test.IsRecording)
            return;
        
        Test.AddPacket(e.Packet);
    }
}