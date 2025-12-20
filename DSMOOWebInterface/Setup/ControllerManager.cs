using System.Reflection;
using System.Text;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Controller;
using DSMOOFramework.Managers;
using EmbedIO;
using EmbedIO.WebApi;

namespace DSMOOWebInterface.Setup;

[Analyze(Priority = 1000000000)]
public class ControllerManager(WebServer webServer, Analyzer analyzer, ObjectController objectController) : Manager
{
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if(!args.Is<WebApiController>() || args.IsAbstract()) return;
        RegisterController(args.Type);
    }

    private void RegisterController(Type type)
    {
        if(!typeof(TemplateController).IsAssignableFrom(type)) return;

        var method = typeof(ControllerManager).GetMethod(nameof(RegisterController),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (method == null) return;
        var genericMethod = method.MakeGenericMethod(type);
        genericMethod.Invoke(this, null);
    }

    private void RegisterController<T>() where T: WebApiController
    {
        //So for whatever stupid reasons does embedio not support generic factories
        webServer.Server.WithWebApi("/", HtmlParser,
            m => m.RegisterController(() => objectController.CreateObject<T>()!));
    }

    private async Task HtmlParser(IHttpContext context, object? data)
    {
        if (data == null) return;
        context.Response.ContentType = MimeType.Html;
        using var text = context.OpenResponseText(new UTF8Encoding(false));
        await text.WriteAsync(data.ToString()).ConfigureAwait(false);
    }
}