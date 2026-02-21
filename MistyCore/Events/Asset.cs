using MistyCore.Models;
using StardewModdingAPI.Events;
using StardewValley;
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
        
        if (e.NameWithoutLocale.BaseName.Equals($"Mods/{ModId}/TileActions/ConditionalMove"))
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

    public static void OnInvalidate(object sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Locations/HoeDirt")))
        {
            HoeDirt = Game1.content.Load<Dictionary<string, string>>($"Mods/{ModId}/Locations/HoeDirt");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Locations/Backgrounds")))
        {
            Backgrounds = Game1.content.Load<Dictionary<string, BackgroundData>>($"Mods/{ModId}/Locations/Backgrounds");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Commands/objectHunt")))
        {
            ObjectHunt = Game1.content.Load<Dictionary<string, HuntContext>>($"Mods/{ModId}/Commands/objectHunt");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Locations/FishingOverrides")))
        {
            FishingOverrides = Game1.content.Load<Dictionary<string, FishingOverrideData>>($"Mods/{ModId}/Locations/FishingOverrides");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/TileActions/AddItem")))
        {
            AddItemTileAction = Game1.content.Load<Dictionary<string, ItemAdditionData>>($"Mods/{ModId}/TileActions/AddItem");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/TileActions/Question")))
        {
            QuestionTileAction = Game1.content.Load<Dictionary<string, QuestionTileActionData>>($"Mods/{ModId}/TileActions/Question");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/TileActions/ConditionalMove")))
        {
            ConditionalWarpTileAction = Game1.content.Load<Dictionary<string, QuestionTileActionData>>($"Mods/{ModId}/TileActions/ConditionalMove");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Locations/Critters")))
        {
            CritterSpawning = Game1.content.Load<Dictionary<string, Dictionary<string,CritterSpawnData>>>($"Mods/{ModId}/Locations/Critters");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Events/OnTreeFall")))
        {
            TreeFallEvents = Game1.content.Load<Dictionary<string, Dictionary<string, ResourceDestroyData>>>($"Mods/{ModId}/Events/OnTreeFall");
        }
        
        if (e.NamesWithoutLocale.Any(asset => asset.Name.Equals($"Mods/{ModId}/Events/OnTreeChop")))
        {
            TreeChopEvents = Game1.content.Load<Dictionary<string, Dictionary<string, ResourceHitData>>>($"Mods/{ModId}/Events/OnTreeChop");
        }
    }
}