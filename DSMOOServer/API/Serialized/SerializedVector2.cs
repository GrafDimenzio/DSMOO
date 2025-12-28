using System.Numerics;

namespace DSMOOServer.API.Serialized;

public class SerializedVector2
{
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;

    public static implicit operator Vector2(SerializedVector2 v) => new(v.X, v.Y);
    public static implicit operator SerializedVector2(Vector2 v) => new() { X = v.X, Y = v.Y };
}