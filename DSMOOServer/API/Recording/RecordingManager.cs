using System.Collections.ObjectModel;
using DSMOOFramework;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
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
    /// <summary>
    /// A Collection of all Recordings that are currently recording a Player
    /// </summary>
    public ReadOnlyDictionary<IPlayer, Recording> ActiveRecordings => _activeRecordings.AsReadOnly();

    /// <summary>
    /// A Collection of all Recordings that are stored to file and ready to be used
    /// </summary>
    public ReadOnlyDictionary<string, Recording> StoredRecordings { get; private set; }

    /// <summary>
    /// Gets a returning based on it's name stored on disk
    /// </summary>
    /// <param name="recordingName"></param>
    /// <returns></returns>
    public Recording? GetRecording(string recordingName)
    {
        return StoredRecordings.GetValueOrDefault(recordingName);
    }

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

    /// <summary>
    ///     This will set the Costume for the Dummy without starting the recording. Player's needs to reload the map first to
    ///     see the costume
    /// </summary>
    /// <param name="recording"></param>
    /// <param name="dummy"></param>
    public async Task SetupDummy(Recording recording, Dummy dummy)
    {
        await dummy.BroadcastPacketAsync(recording.Elements[0].Packet);
    }

    public async Task PlayRecording(Recording recording, Dummy dummy)
    {
        foreach (var element in recording.Elements)
        {
            await Task.Delay(element.Header.Timestamp);
            await dummy.BroadcastPacketAsync(element.Packet);
        }
    }

    public async Task PlayRecording(Recording recording, string name = "Replay")
    {
        var dummy = await dummyManager.CreateDummy(name);
        await PlayRecording(recording, dummy);
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

        StoredRecordings = new ReadOnlyDictionary<string, Recording>(dic);
    }

    public void SaveRecord(string name, Recording recording)
    {
        var path = pathLocation.GetPath("recordings");
        if (path == null)
            return;
        path = Path.Combine(path, name + ".dsmoo");
        recording.SaveToFile(path);
        LoadRecordings();
    }

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        var path = pathLocation.GetPath("recordings");
        if (path != null)
            Directory.CreateDirectory(path);
        LoadRecordings();
    }

    private void OnPacket(PacketReceivedEventArgs e)
    {
        if (!_activeRecordings.TryGetValue(e.Sender.Player, out var recording))
            return;
        recording.AddPacket(e.Packet);
    }
}