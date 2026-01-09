using DSMOOFramework.Commands;
using DSMOOServer.API.Player;
using DSMOOServer.API.Recording;
using DSMOOServer.Logic;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "record",
    Aliases = [""],
    Description = "Commands for recording",
    Parameters = []
)]
public class Record(DummyManager dummyManager, PlayerManager playerManager, RecordingManager recordingManager) : Command
{
    private Recording? _recording;

    public override CommandResult Execute(string command, string[] args)
    {
        if (args.Length < 1)
            return new CommandResult
            {
                ResultType = ResultType.MissingParameter,
                Message = "Usage: record [start/stop/play]"
            };

        switch (args[0].ToLower())
        {
            case "start":
                if (args.Length < 2)
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message = "Usage: record start player"
                    };

                var players = playerManager.SearchForPlayers(args);
                if (players.Players.Count == 0)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No players found"
                    };
                _recording = recordingManager.StartRecording(players.Players[0]);
                return $"Started recording for {players.Players[0].Name}";

            case "stop":
                if (_recording == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.Error,
                        Message = "No active recording"
                    };
                recordingManager.StopRecording(_recording);
                return "Stopped recording";

            case "save":
                if (_recording == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.Error,
                        Message = "You haven't recorded anything yet"
                    };
                if (args.Length < 2)
                    return new CommandResult
                    {
                        ResultType = ResultType.MissingParameter,
                        Message = "Usage: record save (name)"
                    };
                recordingManager.SaveRecord(args[1].ToLower(), _recording);
                return "Saved recording to file " + args[1].ToLower() + ".dsmoo";

            case "play":
                if (args.Length < 2)
                {
                    if (_recording == null)
                        return new CommandResult
                        {
                            ResultType = ResultType.Error,
                            Message = "You haven't recorded anything yet"
                        };

                    recordingManager.PlayRecording(_recording);
                    return "Playing recording";
                }

                var recording = recordingManager.GetRecording(args[1]);
                if (recording == null)
                    return new CommandResult
                    {
                        ResultType = ResultType.InvalidParameter,
                        Message = "No recording with that name found"
                    };
                recordingManager.PlayRecording(recording);
                return "Playing recording";


            case "list":
                return "All recordings:\n    - " + string.Join("\n    - ", recordingManager.StoredRecordings.Keys);

            default:
                return new CommandResult
                {
                    ResultType = ResultType.MissingParameter,
                    Message = "Usage: record [start/stop/play]"
                };
        }
    }
}