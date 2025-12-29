using System.Numerics;

namespace DSMOOServer.API.Serialized;

public class SerializedVector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public static implicit operator Vector2(SerializedVector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator SerializedVector2(Vector2 v)
    {
        return new SerializedVector2 { X = v.X, Y = v.Y };
    }
}