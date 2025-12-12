using DSMOOFramework.Logger;

namespace DSMOOFramework.Events;

public class EventReactor<T>(ILogger logger) where T : IEventArg
{
    public delegate void EventReactorDelegate(T arg);

    private ILogger Logger { get; } = logger;

    public Type EventArgType => typeof(T);

    private event EventReactorDelegate? OnEvent;

    public void Subscribe(EventReactorDelegate handler)
    {
        OnEvent += handler;
    }

    public void Unsubscribe(EventReactorDelegate handler)
    {
        OnEvent -= handler;
    }

    public void RaiseEvent(T e)
    {
        try
        {
            OnEvent?.Invoke(e);
        }
        catch (Exception ex)
        {
            Logger.Error("Error while raising event", ex);
        }
    }
}