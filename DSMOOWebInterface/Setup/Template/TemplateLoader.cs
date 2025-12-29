using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace DSMOOWebInterface.Setup.Template;

public class TemplateLoader(TemplateManager manager) : ITemplateLoader
{
    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return manager.GetFullTemplateName(templateName);
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return manager.Templates.TryGetValue(templatePath, out var template)
            ? template
            : manager.Templates["DSMOOWebInterface.Templates.invalidTemplate.html"];
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return new ValueTask<string>(Load(context, callerSpan, templatePath));
    }
}