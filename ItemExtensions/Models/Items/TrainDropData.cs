namespace ItemExtensions.Models.Items;

internal class TrainDropData : ExtraSpawn
{
    public static CarType Car { get; set; } = CarType.Resource;
    public static ResourceType Type { get; set; } = ResourceType.Coal;
}

public enum CarType
{
    Plain,
    Resource,
    Passenger,
    Engine
}

public enum ResourceType
{
    None,
    Coal,
    Metal,
    Wood,
    Compartments,
    Grass,
    Hay,
    Bricks,
    Rocks,
    Packages,
    Presents
}
