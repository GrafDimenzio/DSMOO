using EmbedIO.WebApi;

namespace DSMOOWebInterface.Setup;

public abstract class TemplateController(TemplateManager manager) : WebApiController
{
    protected async Task<string> RenderTemplate(string templateName, object? model = null)
    {
        return await manager.Render(templateName, model, HttpContext);
    }
}