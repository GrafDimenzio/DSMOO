using System.Reflection;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOWebInterface.Models;
using EmbedIO;
using Scriban;
using Scriban.Runtime;

namespace DSMOOWebInterface.Setup;

public class TemplateManager : Manager
{
    private readonly ILogger _logger;
    private readonly TemplateLoader _loader;
    private readonly List<NavigationElementModel> _navigation = [new()
    {
        Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-house-icon lucide-house\"><path d=\"M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8\"/><path d=\"M3 10a2 2 0 0 1 .709-1.528l7-6a2 2 0 0 1 2.582 0l7 6A2 2 0 0 1 21 10v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\"/></svg>",
        Href = "/",
        Text = "Home",
        SelectionId = 0,
    }, new()
    {
        Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-gauge-icon lucide-gauge\"><path d=\"m12 14 4-4\"/><path d=\"M3.34 19a10 10 0 1 1 17.32 0\"/></svg>",
        Href = "/dashboard",
        Text = "Dashboard",
        SelectionId = 1,
    }, new()
    {
        Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-list-icon lucide-list\"><path d=\"M3 5h.01\"/><path d=\"M3 12h.01\"/><path d=\"M3 19h.01\"/><path d=\"M8 5h13\"/><path d=\"M8 12h13\"/><path d=\"M8 19h13\"/></svg>",
        Href = "/playerlist",
        Text = "Playerlist",
        SelectionId = 2,
    }, new()
    {
        Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-square-terminal-icon lucide-square-terminal\"><path d=\"m7 11 2-2-2-2\"/><path d=\"M11 13h4\"/><rect width=\"18\" height=\"18\" x=\"3\" y=\"3\" rx=\"2\" ry=\"2\"/></svg>",
        Href = "/console",
        Text = "Console",
        SelectionId = 3,
    }, new()
    {
        Svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-map-icon lucide-map\"><path d=\"M14.106 5.553a2 2 0 0 0 1.788 0l3.659-1.83A1 1 0 0 1 21 4.619v12.764a1 1 0 0 1-.553.894l-4.553 2.277a2 2 0 0 1-1.788 0l-4.212-2.106a2 2 0 0 0-1.788 0l-3.659 1.83A1 1 0 0 1 3 19.381V6.618a1 1 0 0 1 .553-.894l4.553-2.277a2 2 0 0 1 1.788 0z\"/><path d=\"M15 5.764v15\"/><path d=\"M9 3.236v15\"/></svg>",
        Href = "/map",
        Text = "Map",
        SelectionId = 4,
    }];
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

    public async Task<string> Render(string templateName, object? model = null, IHttpContext? httpContext = null)
    {
        var context = CreateContext(model, httpContext);
        var templateString = Templates[GetFullTemplateName(templateName)];
        var template = Template.Parse(templateString);
        return await template.RenderAsync(context);
    }

    public void AddNavigation(NavigationElementModel navigation)
    {
        _navigation.Add(navigation);
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

        return Templates.ContainsKey(templateName) ? templateName : "DSMOOWebInterface.Templates.invalidTemplate.html";
    }

    private TemplateContext CreateContext(object? model = null, IHttpContext? httpContext = null)
    {
        var context = new TemplateContext
        {
            TemplateLoader = _loader
        };
        var scriptObject = new ScriptObject();
        
        scriptObject.SetValue("navigation_elements", _navigation, true);

        if (model != null)
        {
            scriptObject.Import(model);
            
        }

        if (httpContext != null)
        {
            foreach (var entry in httpContext.Session.TakeSnapshot())
            {
                scriptObject.SetValue(entry.Key, entry.Value, true);
            }
        }
        
        context.PushGlobal(scriptObject);

        return context;
    }
}