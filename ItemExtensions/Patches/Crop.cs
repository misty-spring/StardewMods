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

    internal static string Cached { get; set; }
    
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
        if (Cached != null)
        {
            __result = Cached;
            return;
        }
        #if DEBUG
        Log($"CALLED ({itemId})");
        #endif
        try
        {
            if (ModEntry.Seeds.TryGetValue(itemId, out var mixedSeeds) == false)
            {
#if DEBUG
                Log($"No data in ModSeeds for {itemId}, checking object custom fields");
#endif
                if (Game1.objectData.TryGetValue(itemId, out var objectData) == false || objectData.CustomFields is null)
                    return;

                if (Game1.objectData[itemId].CustomFields.TryGetValue(ModKeys.MixedSeeds, out var seeds) == false)
                    return;

                //do from custom field
                var splitBySpace = ArgUtility.SplitBySpace(seeds);
                var allFields = new List<string>();

                foreach (var id in splitBySpace)
                {
                    //if not in season:
                    if (Game1.cropData.TryGetValue(id, out var cropData) == false || cropData.Seasons.Contains(Game1.season) == false)
                    {
                        //if no cropAnytime mod
                        if(HasCropsAnytime == false) 
                            continue;
                    
                        //if outdoors
                        if (location.IsOutdoors)
                            continue;
                    }
#if DEBUG
                    Log($"Adding seed id {id}");
#endif
                    switch (id)
                    {
                        case "$vanilla_flowers":
                            allFields.AddRange(GetVanillaFlowersForSeason(Game1.season));
                            break;
                        case "$vanilla_crops":
                            allFields.AddRange(GetVanillaCropsForSeason(Game1.season));
                            break;
                        case "$vanilla_seeds":
                            allFields.AddRange(GetVanillaFlowersForSeason(Game1.season));
                            allFields.AddRange(GetVanillaCropsForSeason(Game1.season));
                            break;
                        default:
                            allFields.Add(id);
                            break;
                    }
                }

                //also add the "main" seed
                for (var i = 0; i < AddMainSeedBy(itemId, allFields); i++)
                {
                    allFields.Add(itemId);
                }

                var fromField = Game1.random.ChooseFrom(allFields);

                Log($"Choosing seed {fromField}");
                Cached = fromField;
                __result = fromField;

                return;
            }

            var all = new List<string>();

            foreach (var seedData in mixedSeeds)
            {
#if DEBUG
                Log($"Checking seed id {seedData.ItemId}");
#endif
                if (!string.IsNullOrWhiteSpace(seedData.Condition) &&
                    GameStateQuery.CheckConditions(seedData.Condition, location, Game1.player) == false)
                    continue;

                //if not in season:
                if (Game1.cropData.TryGetValue(seedData.ItemId, out var cropData) == false || cropData.Seasons.Contains(Game1.season) == false)
                {
                    //if no cropAnytime mod
                    if(HasCropsAnytime == false) 
                        continue;
                    
                    //if outdoors
                    if (location.IsOutdoors)
                        continue;
                }

#if DEBUG
                Log($"Adding seed id {seedData.ItemId} by {seedData.Weight}");
#endif
                //add as many times as weight. e.g, weight 1 gets added once
                for (var i = 0; i < seedData.Weight; i++)
                {
                    switch (seedData.ItemId)
                    {
                        case "$vanilla_flowers":
                            all.AddRange(GetVanillaFlowersForSeason(Game1.season));
                            break;
                        case "$vanilla_crops":
                            all.AddRange(GetVanillaCropsForSeason(Game1.season));
                            break;
                        case "$vanilla_seeds":
                            all.AddRange(GetVanillaFlowersForSeason(Game1.season));
                            all.AddRange(GetVanillaCropsForSeason(Game1.season));
                            break;
                        default:
                            all.Add(seedData.ItemId);
                            break;
                    }
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
            Cached = chosen;
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