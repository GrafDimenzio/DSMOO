using System.Reflection;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Controller;

public class ObjectController
{
    public ILogger Logger { get; }
    
    public readonly Dictionary<Type, Object> Objects = new();
    
    public ObjectController(ILogger logger)
    {
        Logger = logger;
        Objects[typeof(ILogger)] = logger;
        Objects[typeof(ObjectController)] = this;
    }
    
    public void RegisterObject(object obj)
    {
        Objects[obj.GetType()] = obj;
    }

    public object? GetObject(Type type, params object[] overwriteObjects)
    {
        if(Objects.TryGetValue(type, out var o))
            return o;
        
        var obj = CreateObject(type, overwriteObjects);
        if (obj == null)
            return null;
        
        Objects[type] = obj;
        return obj;
    }

    public T? GetObject<T>(params object[] overwriteObjects) where T : class =>
        GetObject(typeof(T), overwriteObjects) as T;

    public object? CreateObject(Type type, params object[] overwriteObjects)
    {
        var constructors = type.GetConstructors();
        var hasEmptyConstructor = false;
        var possibleConstructors = new List<ConstructorInfo>();
        
        foreach (var constructor in constructors)
        {
            if (constructor.GetParameters().Length == 0)
            {
                hasEmptyConstructor = true;
                continue;
            }
            var attr = constructor.GetCustomAttribute<ControllerConstructorAttribute>();
            if (attr != null)
            {
                if (!CanCallConstructor(constructor, overwriteObjects, true))
                {
                    return null;
                }
                return CreateFromConstructor(type, constructor, overwriteObjects);
            }

            if (!CanCallConstructor(constructor, overwriteObjects)) continue;
            possibleConstructors.Add(constructor);
        }

        if (possibleConstructors.Count > 0)
        {
            return CreateFromConstructor(type, possibleConstructors[0], overwriteObjects);
        }

        if (hasEmptyConstructor)
        {
            return Activator.CreateInstance(type);
        }

        Logger.Warn("Cannot find any currently callable constructor for " + type.Name);
        return null;
    }

    private bool CanCallConstructor(ConstructorInfo constructor, object[] overwriteObjects, bool log = false)
    {
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters)
        {
            if (Objects.Keys.Any(x => x == parameter.ParameterType)) continue;
            if (overwriteObjects.Any(x => x.GetType().IsAssignableTo(parameter.ParameterType))) continue;
            if (log)
                Logger.Error(
                    $"Cannot call constructor of {constructor.DeclaringType!.Name} due to missing parameter {parameter.Name}.");
            return false;
        }
        return true;
    }

    private object CreateFromConstructor(Type type, ConstructorInfo constructor, object[] overwriteObjects)
    {
        var objects = new List<object>();
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters)
        {
            var overwriteObject = overwriteObjects.FirstOrDefault(x => x.GetType().IsAssignableTo(parameter.ParameterType));
            if (overwriteObject != null)
            {
                objects.Add(overwriteObject);
                continue;
            }
            
            objects.Add(Objects[parameter.ParameterType]);
        }
        return Activator.CreateInstance(type, objects.ToArray())!;
    }
}