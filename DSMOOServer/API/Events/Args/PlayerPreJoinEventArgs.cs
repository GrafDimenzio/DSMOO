using DSMOOFramework.Events;
using DSMOOServer.Connection;

namespace DSMOOServer.API.Events.Args;

public class PlayerPreJoinEventArgs : IEventArg
{
    public Client Client { get; init; }
    public bool AllowJoin { get; set; }
}