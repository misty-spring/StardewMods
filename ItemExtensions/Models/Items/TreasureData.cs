namespace ItemExtensions.Models.Items;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

public class TreasureData : ExtraSpawn
{
    public string[] Rod { get; set; } = Array.Empty<string>();
    public string[] Bait { get; set; } = Array.Empty<string>();
    public string[] Tackle { get; set; } = Array.Empty<string>();
    public bool RequireAllTackle { get; set; }
    public int Bobber { get; set; } = -1;
    public int MinAttachments { get; set; } = -1;
    public int MaxAttachments { get; set; } = -1;
}
