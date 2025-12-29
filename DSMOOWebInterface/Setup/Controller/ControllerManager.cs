using System.Reflection;
using System.Text;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using EmbedIO;
using EmbedIO.WebApi;

namespace DSMOOWebInterface.Setup.Controller;

[Analyze(Priority = 1000000000)]
public class ControllerManager(
    WebServer webServer,
    Analyzer analyzer,
    ObjectController objectController,
    ILogger logger) : Manager
{
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<WebApiController>() || args.IsAbstract()) return;
        var attribute = args.GetAttribute<ControllerAttribute>();
        if (attribute == null) return;
        RegisterController(args.Type, attribute);
    }

    private void RegisterController(Type type, ControllerAttribute attribute)
    {
        if (!typeof(WebApiController).IsAssignableFrom(type)) return;

        var method = typeof(ControllerManager).GetMethod(nameof(RegisterController),
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(ControllerAttribute)],
            null);

        if (method == null) return;
        var genericMethod = method.MakeGenericMethod(type);
        genericMethod.Invoke(this, [attribute]);
    }

    private void RegisterController<T>(ControllerAttribute attribute) where T : WebApiController
    {
        //So for whatever stupid reasons does embedio not support generic factories
        logger.Setup($"Registering WebController {typeof(T).Name} as {attribute.ControllerType} Type");
        switch (attribute.ControllerType)
        {
            case ControllerType.Html:
                webServer.Server.WithWebApi(attribute.Route, HtmlParser,
                    m => m.RegisterController(() => objectController.CreateObject<T>()!));
                break;

            case ControllerType.Json:
                webServer.Server.WithWebApi(attribute.Route,
                    m => m.RegisterController(() => objectController.CreateObject<T>()!));
                break;
        }
    }

    private async Task HtmlParser(IHttpContext context, object? data)
    {
        if (data == null) return;

        context.Response.ContentType = MimeType.Html;
        await using var text = context.OpenResponseText(new UTF8Encoding(false));
        await text.WriteAsync(data.ToString()).ConfigureAwait(false);
    }
}