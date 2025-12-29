namespace DSMOOFramework;

public class PathLocation
{
    private readonly Dictionary<string, string> _paths = new();

    public string? GetPath(string name)
    {
        return _paths.GetValueOrDefault(name);
    }

    public void AddPath(string name, string path)
    {
        _paths.Add(name, path);
    }

    public void AddPaths(Dictionary<string, string> paths)
    {
        foreach (var path in paths)
            _paths.Add(path.Key, path.Value);
    }
}