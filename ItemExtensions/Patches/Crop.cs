using HarmonyLib;
using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace ItemExtensions.Patches;

internal class CropPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static bool HasCropsAnytime { get; set; }
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
        try
        {
            if (ModEntry.Seeds.TryGetValue(itemId, out var mixedSeeds) == false)
            {
                if (Game1.objectData[itemId].CustomFields is null)
                    return;

                if (Game1.objectData[itemId].CustomFields.TryGetValue(ModKeys.MixedSeeds, out var seeds) == false)
                    return;

                //do from custom field
                var splitBySpace = ArgUtility.SplitBySpace(seeds);
                var allFields = new List<string>();

                foreach (var id in splitBySpace)
                {
                    //if season is allowed, has crops anytime OR in greenhouse
                    if (Game1.cropData[id].Seasons.Contains(Game1.season) || HasCropsAnytime || (Game1.season == Season.Winter && location.IsGreenhouse))
                        allFields.Add(id);
                }

                //also add the "main" seed
                for (var i = 0; i < AddMainSeedBy(itemId, allFields); i++)
                {
                    allFields.Add(itemId);
                }

                var fromField = Game1.random.ChooseFrom(allFields);

                Log($"Choosing seed {fromField}");
                __result = fromField;

                return;
            }

            var all = new List<string>();

            foreach (var seedData in mixedSeeds)
            {
                if (!string.IsNullOrWhiteSpace(seedData.Condition) &&
                    GameStateQuery.CheckConditions(seedData.Condition, location, Game1.player) == false)
                    continue;

                /* the seed will be skipped if all of these are valid:
                 * not in season
                 * winter and not in greenhouse
                 * doesn't have CropsAnytime
                 * is outdoors and NOT in island context
                 */
                if (Game1.cropData[seedData.ItemId].Seasons.Contains(Game1.season) == false && (Game1.season == Season.Winter && !location.IsGreenhouse) && HasCropsAnytime == false && (Game1.player.currentLocation.IsOutdoors && Game1.player.currentLocation.InIslandContext() == false))
                    continue;

                //add as many times as weight. e.g, weight 1 gets added once
                for (var i = 0; i < seedData.Weight; i++)
                {
                    all.Add(seedData.ItemId);
                }
            }

            //also add the "main" seed
            for (var i = 0; i < AddMainSeedBy(itemId, all); i++)
            {
                all.Add(itemId);
            }

            //if none in all, return. shouldn't happen but just in case
            if (all.Any() == false)
                return;


            if (itemId == "MixedFlowerSeeds")
            {
                all.AddRange(GetVanillaFlowersForSeason(location.GetSeason()));
            }
            else if (itemId == "770")
            {
                all.AddRange(GetVanillaCropsForSeason(location.GetSeason()));
            }

            //if there's none in Add, fallback to random crop (shouldn't happen but still)
            if (all.Count <= 0 || all.Any() == false)
            {
                all.Add(Crop.getRandomLowGradeCropForThisSeason(location.GetSeason()));
            }
            
            var chosen = Game1.random.ChooseFrom(all);
            Log($"Choosing seed {chosen}");
            __result = chosen;
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static string[] GetVanillaFlowersForSeason(Season season)
    {
        if (season == Season.Winter)
            season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
        return season switch
        {
            Season.Spring => new[] { "427", "429" },
            Season.Summer => new[] { "455", "453", "431" },
            Season.Fall => new[] { "431", "425" },
            _ => Array.Empty<string>()
        };
    }

    private static string[] GetVanillaCropsForSeason(Season season)
    {
        if (season == Season.Winter)
            season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
        return season switch
        {
            Season.Spring => new[] { "472", "476" },
            Season.Summer => new[] { "487", "483", "482", "484" },
            Season.Fall => new[] { "487", "491" },
            _ => Array.Empty<string>()
        };
    }

    private static int AddMainSeedBy(string itemId, List<string> allFields)
    {
        var fields = Game1.objectData[itemId].CustomFields;
        
        // if empty, return "once"
        if (allFields is null || allFields.Any() == false)
            return 0;
        
        // if there's any custom fields
        if (fields is not null && fields.Any())
        {
            // if it has specific count
            if (fields.TryGetValue(ModKeys.AddMainSeed, out var timesToAdd))
            {
                return int.Parse(timesToAdd);
            }
        }

        return 0;
    }
}