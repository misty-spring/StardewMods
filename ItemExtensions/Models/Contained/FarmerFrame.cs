namespace ItemExtensions.Models.Contained;

public class FarmerFrame
{
    public int Frame { get; set; } = 0;
    public int Duration { get; set; } = 200;
    public bool SecondaryArm { get; set; } = false;
    public bool Flip { get; set; } = false;
    public bool HideArm { get; set; } = false;
}