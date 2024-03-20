namespace ItemExtensions.Models;

public class ResourceData
{
    /// <summary>
    /// The stone's health. Every hit reduces UpgradeLevel + 1.
    /// For weapons, it does 10% average DMG + 1.
    /// See <see cref="StardewValley.Locations.MineShaft"/> for stone health.
    /// </summary>
    public int Health { get; set; } = 10;
    public string Sound { get; set; } = "hammer";
    public string BreakingSound { get; set; } = "stoneCrack";
    public string Debris { get; set; } = "stone";

    public string ItemDropped { get; set; } = null;

    /// <summary>
    /// Tool's class. In the case of weapons, it can also be its number.
    /// </summary>
    public string Tool { get; set; } = null;
    /// <summary>
    /// Minimum upgrade tool should have. If a weapon, the minimum number is checked. 
    /// ("number": 10% of average damage)
    /// </summary>
    public int MinToolLevel { get; set; } = 0;
    
    public int MinDrops { get; set; } = 1;
    public int MaxDrops { get; set; } = 1;

    public List<ExtraSpawn> ExtraItems { get; set; } = null;
}