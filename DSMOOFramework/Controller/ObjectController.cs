using System.Reflection;
using DSMOOFramework.Logger;

namespace DSMOOFramework.Controller;

public class ObjectController
{
    public readonly Dictionary<Type, object> Objects = new();

    public ObjectController(ILogger logger)
    {
        Logger = logger;
        Objects[typeof(ILogger)] = logger;
        Objects[typeof(ObjectController)] = this;
    }

    public ILogger Logger { get; }

    public void RegisterObject(object obj)
    {
        Objects[obj.GetType()] = obj;
    }

    public object? GetObject(Type type, params object[] overwriteObjects)
    {
        if (Objects.TryGetValue(type, out var o))
            return o;

        var obj = CreateObject(type, overwriteObjects);
        if (obj == null)
            return null;

        Objects[type] = obj;
        return obj;
    }

    public T? GetObject<T>(params object[] overwriteObjects) where T : class
    {
        return GetObject(typeof(T), overwriteObjects) as T;
    }

    public T? CreateObject<T>(params object[] overwriteObjects) => (T?)CreateObject(typeof(T), overwriteObjects);
    
    public object? CreateObject(Type type, params object[] overwriteObjects)
    {
        var constructors = type.GetConstructors();
        var hasEmptyConstructor = false;
        var possibleConstructors = new List<ConstructorInfo>();

        foreach (var constructor in constructors)
        {
            var attr = constructor.GetCustomAttribute<ControllerConstructorAttribute>();
            if (attr != null) return CreateFromConstructor(type, constructor, overwriteObjects);

            if (constructor.GetParameters().Length == 0)
            {
                hasEmptyConstructor = true;
                continue;
            }

            possibleConstructors.Add(constructor);
        }

        foreach (var constructor in possibleConstructors)
            try
            {
                return CreateFromConstructor(type, constructor, overwriteObjects);
            }
            catch (Exception e)
            {
                Logger.Error("Error while using Constructor", e);
            }

        if (hasEmptyConstructor)
        {
            var result = Activator.CreateInstance(type)!;
            InjectObject(result);
            return result;
        }

        Logger.Warn("Cannot find any currently callable constructor for " + type.Name);
        return null;
    }

    public void InjectObject(object obj)
    {
        foreach (var field in obj.GetType().GetFields())
        {
            if (field.GetCustomAttribute<InjectAttribute>() == null) continue;
            field.SetValue(obj, GetObject(field.FieldType));
        }

        foreach (var property in obj.GetType().GetProperties())
        {
            if (property.GetCustomAttribute<InjectAttribute>() == null) continue;
            property.SetValue(obj, GetObject(property.PropertyType));
        }
        
        if(obj is IInject inject)
            inject.AfterInject();
    }

    private object CreateFromConstructor(Type type, ConstructorInfo constructor, object[] overwriteObjects)
    {
        var objects = new List<object>();
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters)
        {
            var overwriteObject =
                overwriteObjects.FirstOrDefault(x => x.GetType().IsAssignableTo(parameter.ParameterType));
            if (overwriteObject != null)
            {
                objects.Add(overwriteObject);
                continue;
            }

            if (Objects.TryGetValue(parameter.ParameterType, out var o))
            {
                objects.Add(o);
            }
            else
            {
                var obj = GetObject(parameter.ParameterType);
                if (obj == null)
                    throw new Exception(
                        $"Can't create object of type {type} since Object of Type {parameter.ParameterType} cannot be created for it's constructor.");
                objects.Add(obj);
            }
        }
        
        var result = Activator.CreateInstance(type, objects.ToArray())!;
        InjectObject(result);
        return result;
    }
}