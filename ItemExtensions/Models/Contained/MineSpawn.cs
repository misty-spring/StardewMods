using ItemExtensions.Models.Enums;

namespace ItemExtensions.Models.Contained;

public class MineSpawn
{
    public MineSpawn()
    {
    }

    public MineSpawn(IEnumerable<string> floors, double spawnFrequency, double additionalChancePerLevel, bool main)
    {
        Type = main ? MineType.General : MineType.All;
        RealFloors = floors as string[];
        SpawnFrequency = spawnFrequency;
        AdditionalChancePerLevel = additionalChancePerLevel;
    }

    public string Floors { get; set; } = null;
    public MineType Type { get; set; } = MineType.All;
    public string Condition { get; set; } = null;
    internal string[] RealFloors { get; set; } = Array.Empty<string>();
    public double SpawnFrequency { get; set; } = 0.1;
    public double AdditionalChancePerLevel { get; set; }

    public void Parse(IEnumerable<string> floors)
    {
        RealFloors = floors as string[];
    }
}