namespace DynamicFestivalRewards;

internal class ModConfig
{
    public bool UseYearInstead { get; set; }
    public bool Randomize { get; set; } = true;
    public int MinValue { get; set; } = 200;
    public int MaxValue { get; set; } = 1000;
}