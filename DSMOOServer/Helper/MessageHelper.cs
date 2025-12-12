using System.Text;
using DSMOOFramework.Commands;
using DSMOOServer.Logic;

namespace DSMOOServer.Helper;

public class MessageHelper
{
    public static CommandResult FormatMessage(PlayerManager.PlayerSearch players, string message)
    {
        if (players.Players.Count == 0 && players.FailedToFind.Count == 0 && players.MultipleMatches.Count == 0)
            return new CommandResult
            {
                ResultType = ResultType.InvalidParameter,
                Message = "No players where found with that name or id"
            };

        var sb = new StringBuilder();
        sb.Append(players.Players.Count > 0
            ? $"{message}: {string.Join(", ", players.Players.Select(x => x.Name))}\n"
            : "");
        sb.Append(players.FailedToFind.Count > 0
            ? $"Failed to find matches for: {string.Join(", ", players.FailedToFind)}\n"
            : "");
        foreach (var match in players.MultipleMatches)
            sb.Append($"Ambiguous for {match.Item1}: {string.Join(", ", match.Item2.Select(x => x.Name))}\n");

        return new CommandResult
        {
            ResultType = players.Players.Count > 0 ? ResultType.Success : ResultType.InvalidParameter,
            Message = sb.ToString().Trim('\n')
        };
    }
}