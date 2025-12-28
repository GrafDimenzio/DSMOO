using System.Reflection;
using DSMOOConsole;
using DSMOOFramework;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOServer.Connection;

ConsoleSetup("Server");
Task.Delay(-1).GetAwaiter().GetResult();

static ObjectController ConsoleSetup(string loggerName)
{
    var controller = SetupHelper.BasicSetup(new ConsoleLogger(GetLogTypes()) { Name = loggerName }, new Dictionary<string, string>
    {
        { "config", Path.Combine(AppContext.BaseDirectory, "configs") },
        { "plugins", Path.Combine(AppContext.BaseDirectory, "plugins") },
        { "recordings", Path.Combine(AppContext.BaseDirectory, "recordings") }
    }, [Assembly.GetAssembly(typeof(Server))!]);
    var command = controller.GetObject<ConsoleCommands>();
    Task.Run(() => command?.ListenForCommands());
    return controller;
}

static LogType[] GetLogTypes()
{
    var args = Environment.GetCommandLineArgs();
    var types = new List<LogType> { LogType.Error, LogType.Info, LogType.Warn};
    
    if(args.Contains("-setup"))
        types.Add(LogType.Setup);
    
    if(args.Contains("-debug"))
        types.Add(LogType.Debug);
    
    return types.ToArray();
}