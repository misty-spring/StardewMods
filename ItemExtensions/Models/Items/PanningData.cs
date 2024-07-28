namespace ItemExtensions.Models.Items;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

public class PanningData : ExtraSpawn
{
    public int MinUpgrade { get; set; } = 0;
    public int MaxUpgrade { get; set; } = -1;
}