namespace DSMOOFramework.Config;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConfigAttribute : Attribute
{
    public string Name { get; set; } = "";
}