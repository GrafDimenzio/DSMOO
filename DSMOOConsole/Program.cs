using System.Reflection;
using DSMOOConsole;
using DSMOOFramework;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOServer.Connection;


var controller = ConsoleSetup("Server");
controller.GetObject<Analyzer>()?.AnalyzeAssembly(Assembly.GetAssembly(typeof(Server)));
Task.Delay(-1).GetAwaiter().GetResult();

static ObjectController ConsoleSetup(string loggerName)
{
    var controller = SetupHelper.BasicSetup(new ConsoleLogger() { Name = loggerName });
    var command = controller.GetObject<ConsoleCommands>();
    Task.Run(() => command?.ListenForCommands());
    return controller;
}