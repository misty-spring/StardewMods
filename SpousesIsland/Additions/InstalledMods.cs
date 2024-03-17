namespace SpousesIsland.Additions;

internal class InstalledMods
{
    public bool StardewExpanded { get; set; } = HasMod("FlashShifter.StardewValleyExpandedCP");
    public bool LittleNpcs { get; set; } = HasMod("Candidus42.LittleNPCs");
    public bool Devan { get; set; } = HasMod("mistyspring.DevanNPC");
    public bool LnhIsland { get; set; } = HasMod("Lnh.IslandOverhaul");

    internal static bool HasMod(string modId)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            ModEntry.Mon.Log("Mod Id can't be empty!");
            return false;
        }
        
        return ModEntry.Help.ModRegistry.Get(modId) is not null;
    }
}
