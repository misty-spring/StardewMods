using ItemExtensions.Models.Enums;
using StardewValley;

namespace ItemExtensions.Models.Contained;

public class MineSpawn
{
    public MineSpawn()
    {
    }

    public MineSpawn(IEnumerable<string> floors, double spawnFrequency, double additionalChancePerLevel, bool main)
    {
        Type = main ? MineType.General : MineType.All;
        RealFloors = floors as List<string>;
        SpawnFrequency = spawnFrequency;
        AdditionalChancePerLevel = additionalChancePerLevel;
    }

    public string Floors { get; set; } = null;
    public MineType Type { get; set; } = MineType.All;
    public string Condition { get; set; } = null;
    internal List<string> RealFloors { get; set; } = new();
    public double SpawnFrequency { get; set; } = 0.1;
    public double AdditionalChancePerLevel { get; set; }
    internal (int, Season) LastFrenzy { get; set; } = (-1, Season.Spring);

    public void Parse(IEnumerable<string> floors)
    {
        RealFloors = floors as List<string>;
    }
}
