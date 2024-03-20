using HarmonyLib;
using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace ItemExtensions.Patches;

public class CropPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(CropPatches)}\": postfixing SDV method \"Crop.ResolveSeedId(string, GameLocation)\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Crop), nameof(Crop.ResolveSeedId)),
            postfix: new HarmonyMethod(typeof(CropPatches), nameof(Post_ResolveSeedId))
        );
    }

    /// <summary>Choose a random seed from a bag of mixed seeds, if applicable.</summary>
    /// <param name="itemId">The unqualified item ID for the seed item.</param>
    /// <param name="location">The location for which to resolve the crop.</param>
    /// <param name="__result">The unqualified seed ID to use.</param>
    public static void Post_ResolveSeedId(string itemId, GameLocation location, ref string __result)
    {
        if (ModEntry.Seeds.TryGetValue(itemId, out var mixedSeeds) == false)
        {
            if(Game1.objectData[itemId].CustomFields is null)
                return;
            
            if(Game1.objectData[itemId].CustomFields.TryGetValue(ModKeys.MixedSeeds, out var seeds) == false)
                return;
            
            //do from custom field
            var splitBySpace = ArgUtility.SplitBySpace(seeds);
            var allFields = new List<string>();
            
            foreach (var id in splitBySpace)
            {
                if(Game1.cropData[id].Seasons.Contains(Game1.season))
                    allFields.Add(id);
            }
            
            //this checks if seed roster should exclude our seed. if there's no valid seeds, the main seed will be force-added
            if(ShouldAddMainSeed(itemId, allFields))
                allFields.Add(itemId);
            
            var fromField = Game1.random.ChooseFrom(allFields);
            
            Log($"Choosing seed {fromField}");
            __result = fromField;
            
            return;
        }

        var all = new List<string>();

        foreach (var seedData in mixedSeeds)
        {
            if(!string.IsNullOrWhiteSpace(seedData.Condition) && GameStateQuery.CheckConditions(seedData.Condition, location, Game1.player) == false)
                continue;
            
            //if outdoors AND not in island
            if (Game1.player.currentLocation.IsOutdoors && Game1.player.currentLocation.InIslandContext() == false)
            {
                //if season not allowed
                if(Game1.cropData[seedData.ItemId].Seasons.Contains(Game1.season) == false)
                    continue;
            }

            //add as many times as weight. e.g, weight 1 gets added once
            for (var i = 0; i < seedData.Weight; i++)
            {
                all.Add(seedData.ItemId);
            }
        }
        
        //also add the "main" seed
        if(ShouldAddMainSeed(itemId, all))
            all.Add(itemId);

        //if none in all, return. shouldn't happen but just in case
        if (all.Any() == false)
            return;

        var chosen = Game1.random.ChooseFrom(all);
        Log($"Choosing seed {chosen}");
        __result = chosen;
    }

    private static bool ShouldAddMainSeed(string itemId, List<string> allFields)
    {
        var fields = Game1.objectData[itemId].CustomFields;
        
        // if empty, return false
        if (allFields.Any() == false)
            return false;
        
        // if there's any custom fields
        if (fields is not null && fields.Any())
        {
            // if it doesn't have the Exclude field, add
            if(fields.TryGetValue(ModKeys.ExcludeFromSeedRoster, out var shouldExclude) == false)
            {
                return true;
            }
            
            // otherwise, make "should add" return the opposite of "should exclude"
            // here it checks if it equals false, but basically same intent
            return bool.Parse(shouldExclude) == false;
        }

        return true;
    }
}