namespace DSMOOFramework.Analyzer;

[AttributeUsage(AttributeTargets.All)]
public class AnalyzeAttribute : Attribute
{
    public int Priority { get; set; } = 0;
}