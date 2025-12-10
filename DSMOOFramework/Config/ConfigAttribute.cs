using DSMOOFramework.Analyzer;

namespace DSMOOFramework.Config;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConfigAttribute : AnalyzeAttribute
{
    public ConfigAttribute()
    {
        Priority = int.MaxValue;
    }
    public string Name { get; set; } = "";
}