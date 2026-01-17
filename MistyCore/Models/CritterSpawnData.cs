namespace MistyCore.Models;

public class CritterSpawnData
{
    public float Chance { get; set; } = 1;
    public string Condition { get; set; } = "TRUE";
    public int X { get; set; }
    public int Y { get; set; }
    public CritterType Critter { get; set; } = CritterType.Butterfly;

    //specific to butterflies
    public bool IslandButterfly { get; set; }
    public bool ForceSummerButterfly { get; set; }

    //specific to some
    public bool Flip { get; set; }

    //specific to seagulls
    public SeagullBehavior SeagullBehavior { get; set; } = SeagullBehavior.Walking;
}

public enum CritterType
{
    //Bird,
    Butterfly,
    CalderaMonkey,
    CrabCritter,
    Crow,
    Firefly,
    Frog,
    Opossum,
    OverheadParrot,
    Owl,
    Rabbit,
    Seagull,
    Squirrel
    //Woodpecker
}

public enum SeagullBehavior
{
    Walking,
    FlyingAway,
    Swimming,
    Stopped,
    FlyingToLand
}