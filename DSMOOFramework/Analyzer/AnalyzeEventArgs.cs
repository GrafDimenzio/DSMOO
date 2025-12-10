using System.Reflection;
using DSMOOFramework.Events;

namespace DSMOOFramework.Analyzer;

public class AnalyzeEventArgs : IEventArg
{
    public AnalyzeEventArgs(Type type)
    {
        Type = type;
        var attribute = type.GetCustomAttributes<AnalyzeAttribute>().FirstOrDefault();
        Priority = attribute?.Priority ?? 0;
    }
    
    public Type Type { get; }
    
    public int Priority { get; }
    
    public bool HasAttribute(Type attributeType) => Type.GetCustomAttributes(attributeType, true).Length > 0;
    
    public bool HasAttribute<T>() => Type.GetCustomAttributes(typeof(T), true).Length > 0;
    
    public T? GetAttribute<T>() where T : Attribute => Type.GetCustomAttribute<T>(true);
    
    public bool Is<T>() => typeof(T).IsAssignableFrom(Type);
}