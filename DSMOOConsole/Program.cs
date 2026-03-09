using System.Diagnostics;
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
    var controller = SetupHelper.BasicSetup(new ConsoleLogger(GetLogTypes()) { Name = loggerName },
        GetPaths(), [Assembly.GetAssembly(typeof(Server))!]);
    var command = controller.GetObject<ConsoleCommands>();
    Task.Run(() => command?.ListenForCommands());
    return controller;
}

static Dictionary<string, string> GetPaths()
{
    if (File.Exists("/.dockerenv"))
        return new Dictionary<string, string>()
        {
            { "config", "/dsmoo/configs" },
            { "plugins", "/dsmoo/plugins" },
            { "recordings", "/dsmoo/recordings" },
            { "mods", "/dsmoo/mods" },
        };

    var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
    return new Dictionary<string, string>
    {
        { "config", Path.Combine(path, "configs") },
        { "plugins", Path.Combine(path, "plugins") },
        { "recordings", Path.Combine(path, "recordings") },
        { "mods", Path.Combine(path, "mods") },
    };
}

static LogType[] GetLogTypes()
{
    var args = Environment.GetCommandLineArgs();
    var types = new List<LogType> { LogType.Error, LogType.Info, LogType.Warn };

    if (args.Contains("-setup"))
        types.Add(LogType.Setup);

    if (args.Contains("-debug"))
        types.Add(LogType.Debug);

    return types.ToArray();
}