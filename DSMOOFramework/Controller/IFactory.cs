namespace DSMOOFramework.Controller;

public interface IFactory<out T>
{
    public T Create(Type createFor);
}