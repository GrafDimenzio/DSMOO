using DSMOOFramework.Commands;
using DSMOOServer.API.Player;
using DSMOOServer.API.Recording;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "dummy",
    Aliases = [""],
    Description = "Dummy Command",
    Parameters = []
)]
public class DummyCommand(DummyManager dummyManager, PlayerManager playerManager, RecordingManager recordingManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        var recording = new Recording("recording.dsmoo");
        recordingManager.PlayRecording(recording);
        return "PLAY";
        
        if (!recordingManager.Test.IsRecording)
        {
            recordingManager.Test.StartRecording(playerManager.Players.First(x => !x.IsDummy));
            return "Start";
        }
        
        recordingManager.Test.StopRecording();
        recordingManager.PlayRecording(recordingManager.Test);
        recordingManager.Test.SaveToFile("recording.dsmoo");

        return "Play";
    }
}