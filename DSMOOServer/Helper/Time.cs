namespace DSMOOServer.Helper;

public record Time(ushort Minutes, byte Seconds, DateTime When)
{
    public static implicit operator Time((ushort, byte) tuple)
    {
        return new Time(tuple.Item1, tuple.Item2, DateTime.Now);
    }

    public static implicit operator Time((byte, ushort) tuple)
    {
        return new Time(tuple.Item2, tuple.Item1, DateTime.Now);
    }
}