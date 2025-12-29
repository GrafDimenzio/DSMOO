namespace DSMOOFlip;

[Flags]
public enum FlipOptions
{
    Self = 1,
    Other = 2,
    Both = Self | Other
}