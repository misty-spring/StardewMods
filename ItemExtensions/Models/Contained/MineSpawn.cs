using ItemExtensions.Models.Enums;

namespace ItemExtensions.Models.Contained;

public class MineSpawn
{
    public MineSpawn()
    {
    }
    
    public MineSpawn(IEnumerable<string> floors, bool main)
    {
        Type = main ? MineType.General : MineType.All;
        RealFloors = floors as string[];
    }

    public string Floors { get; set; } = null;
    public MineType Type { get; set; } = MineType.All;
    public string Condition { get; set; } = null;
    //public bool ExcludeGeneralLevels { get; set; }
    internal string[] RealFloors { get; set; }

    public void Parse(IEnumerable<string> floors)
    {
        RealFloors = floors as string[];
    }
}