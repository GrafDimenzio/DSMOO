using System.Reflection;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using EmbedIO;
using Scriban;
using Scriban.Runtime;

namespace DSMOOWebInterface.Setup;

public class TemplateManager : Manager
{
    private readonly ILogger _logger;
    private readonly TemplateLoader _loader;
    internal readonly Dictionary<string, string> Templates = [];

    public TemplateManager(ILogger logger)
    {
        _logger = logger;
        _loader = new TemplateLoader(this);
    }
    
    public override void Initialize()
    {
        LoadTemplates(GetType().Assembly, "DSMOOWebInterface.Templates");
    }

    public async Task<string> Render(string templateName, object? model = null, IHttpContext httpContext = null)
    {
        var context = CreateContext(model, httpContext);
        var templateString = Templates[GetFullTemplateName(templateName)];
        var template = Template.Parse(templateString);
        return await template.RenderAsync(context);
    }

    public void LoadTemplates(Assembly assembly, string path = "")
    {
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (!name.StartsWith(path))
                continue;
            try
            {
                var stream = assembly.GetManifestResourceStream(name);
                using var reader = new StreamReader(stream!);
                var txt = reader.ReadToEnd();
                Templates[name] = txt;
                _logger.Setup("Loaded Template " + name);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occured while reading Templates from Assembly {assembly.FullName}", ex);
            }
        }
    }
    
    internal string GetFullTemplateName(string templateName)
    {
        var possible = Templates.Where(x => x.Key.EndsWith(templateName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (possible.Count == 1)
            return possible[0].Key;

        return Templates.ContainsKey(templateName) ? templateName : "DSMOOWebInterface.Templates.InvalidTemplate.html";
    }

    private TemplateContext CreateContext(object? model = null, IHttpContext? httpContext = null)
    {
        var context = new TemplateContext
        {
            TemplateLoader = _loader
        };

        if (model != null)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(model);
            context.PushGlobal(scriptObject);
        }

        if (httpContext != null)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(httpContext.Session);
            context.PushGlobal(scriptObject);
        }

        return context;
    }
}