namespace DSMOOFlip;

[Flags]
public enum FlipOptions : int
{
    Self = 1,
    Other = 2,
    Both = Self | Other
}