using System.Collections.ObjectModel;
using DSMOOFramework;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
using DSMOOServer.Logic;
using DSMOOServer.Network;

namespace DSMOOServer.API.Recording;

public class RecordingManager(
    DummyManager dummyManager,
    EventManager eventManager,
    PacketManager packetManager,
    ILogger logger,
    PathLocation pathLocation) : Manager
{
    private readonly Dictionary<IPlayer, Recording> _activeRecordings = new();
    public ReadOnlyDictionary<IPlayer, Recording> ActiveRecordings => _activeRecordings.AsReadOnly();

    public ReadOnlyDictionary<string, Recording> StoredRecordings { get; private set; }

    public Recording StartRecording(IPlayer player)
    {
        var recording = new Recording(packetManager);
        recording.StartRecording(player);
        _activeRecordings[player] = recording;
        return recording;
    }

    public void StopRecording(Recording recording)
    {
        foreach (var pair in _activeRecordings.ToList())
            if (pair.Value == recording)
                _activeRecordings.Remove(pair.Key);
        recording.StopRecording();
    }

    public async Task PlayRecording(Recording recording, string name = "Replay")
    {
        var dummy = await dummyManager.CreateDummy(name);

        foreach (var element in recording.Elements)
        {
            await Task.Delay(element.Header.Timestamp);
            await dummy.BroadcastPacket(element.Packet);
        }

        dummy.Dispose();
    }

    public void LoadRecordings()
    {
        var path = pathLocation.GetPath("recordings");
        if (path == null)
            return;
        var dic = new Dictionary<string, Recording>();
        foreach (var file in Directory.GetFiles(path))
        {
            var recording = new Recording(file, packetManager);
            dic[Path.GetFileNameWithoutExtension(file)] = recording;
        }
    }

    public void SaveRecord(string name, Recording recording)
    {
        var path = pathLocation.GetPath("recordings");
        if (path == null)
            return;
        path = Path.Combine(path, name + ".dsmoo");
        recording.SaveToFile(path);
    }

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        var path = pathLocation.GetPath("recordings");
        if (path != null)
            Directory.CreateDirectory(path);
    }

    private void OnPacket(PacketReceivedEventArgs e)
    {
        if (!_activeRecordings.TryGetValue(e.Sender.Player, out var recording))
            return;
        recording.AddPacket(e.Packet);
    }
}