namespace DSMOOFramework.Commands;

public class CommandResult
{
    public string Message { get; set; } = "";
    public ResultType ResultType { get; set; } = ResultType.Success;

    public static implicit operator CommandResult(string message)
    {
        return new CommandResult { Message = message };
    }
    
    public static implicit operator CommandResult((string, ResultType) result)
    {
        return new CommandResult { Message = result.Item1, ResultType = result.Item2 };
    }
}