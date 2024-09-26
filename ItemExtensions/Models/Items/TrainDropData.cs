namespace ItemExtensions.Models.Items;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

public class TrainDropData : ExtraSpawn
{
    public CarType Car { get; set; } = CarType.Resource;
    public ResourceType Type { get; set; } = ResourceType.Coal;
    public double ChanceDropOnFront { get; set; } = 0.5;
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
