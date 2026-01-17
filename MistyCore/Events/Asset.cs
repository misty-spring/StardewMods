using MistyCore.Models;
using StardewModdingAPI.Events;
using static MistyCore.ModEntry;

namespace MistyCore.Events;

public static class Asset
{
    public static void OnRequest(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Locations/HoeDirt"))
        {
            e.LoadFrom(
                () => new Dictionary<string, string>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Locations/Backgrounds"))
        {
            e.LoadFrom(
                () => new Dictionary<string, BackgroundData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Commands/objectHunt"))
        {
            e.LoadFrom(
                () => new Dictionary<string, HuntContext>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Locations/FishingOverrides"))
        {
            e.LoadFrom(
                () => new Dictionary<string, FishingOverrideData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/TileActions/AddItem"))
        {
            e.LoadFrom(
                () => new Dictionary<string, ItemAdditionData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/TileActions/Question"))
        {
            e.LoadFrom(
                () => new Dictionary<string, QuestionTileActionData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/TileActions/ConditionalWarp"))
        {
            e.LoadFrom(
                () => new Dictionary<string, QuestionTileActionData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Locations/Critters"))
        {
            e.LoadFrom(
                () => new Dictionary<string, Dictionary<string,CritterSpawnData>>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Events/OnTreeFall"))
        {
            e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, ResourceDestroyData>>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/Events/OnTreeChop"))
        {
            e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, ResourceHitData>>(),
                AssetLoadPriority.Low);
        }
    }
}