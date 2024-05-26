namespace ItemExtensions.Models.Items;

public class TrainDropData : ExtraSpawn
{
    public CarType Car { get; set; } = CarType.Resource;
    public ResourceType Type { get; set; } = ResourceType.Coal;
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
