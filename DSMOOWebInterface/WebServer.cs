using System.Text;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Plugins;
using DSMOOWebInterface.Setup;
using EmbedIO;
using Swan.Logging;

namespace DSMOOWebInterface;

[Analyze(Priority = int.MinValue)]
[Plugin(
    Name = "DSMOOWebInterface",
    Author = "Dimenzio",
    Description = "A Plugin that hosts a WebInterface",
    Repository = "https://github.com/GrafDimenzio/DSMOO",
    Version = "1.0.0"
    )]
public class WebServer(TemplateManager templateManager, ObjectController objectController) : Plugin<Config>
{
    public EmbedIO.WebServer Server { get; internal set; }

    public string FullUrl => $"{Config.Url}:{Config.Port}";
    
    public override void Initialize()
    {
        Server.RunAsync();
    }
    
    public override void AfterInject()
    {
#if !DEBUG
        //While Debugging the original ConsoleLogger is better
        Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
        Swan.Logging.Logger.RegisterLogger(objectController.GetObject<LogConverter>()!);
#endif
        
        //The WebServer object is created instantly, however it is only started after all controllers are registered
        Server = CreateWebServer();
    }

    private EmbedIO.WebServer CreateWebServer()
    {
        var server = new EmbedIO.WebServer(x =>
                x.WithUrlPrefix(FullUrl)
                    .WithMode(HttpListenerMode.EmbedIO))
            .WithEmbeddedResources("/static", GetType().Assembly, "DSMOOWebInterface.wwwroot")
            .WithLocalSessionManager()
            .HandleHttpException(HttpException);
        
        server.StateChanged += (sender, args) => Logger.Info($"WebServer New State - {args.NewState}");
        return server;
    }

    private async Task HttpException(IHttpContext context, IHttpException httpException)
    {
        context.Response.ContentType = "text/html";
        var html = await templateManager.Render("DSMOOWebInterface.Templates.exception.html", httpException);
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(html));
        context.SetHandled();
    }
}