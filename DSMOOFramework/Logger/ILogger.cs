namespace DSMOOFramework.Logger;

public interface ILogger
{
    string Name { get; set; }

    void Info(object message);

    void Error(object message);

    void Error(object? message, Exception ex);

    void Warn(object message);

    void Setup(object message);

    void Debug(object message);

    ILogger Copy();
}