using System.Text;
using HarmonyLib;
using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;

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
        Log($"Applying Harmony patch \"{nameof(CropPatches)}\": prefixing SDV constructor \"Crop(string, int, int, GameLocation)\".");
        
        harmony.Patch(
            original: AccessTools.Constructor(typeof(Crop), new[] {typeof(string), typeof(int), typeof(int), typeof(GameLocation)}),
            prefix: new HarmonyMethod(typeof(CropPatches), nameof(Pre_Constructor))
        );
    }

    internal static void Pre_Constructor (Crop __instance, ref string seedId, int tileX, int tileY, GameLocation location)
    {
        if (string.IsNullOrWhiteSpace(seedId))
            return;

        if (Cached != null)
        {
            seedId = Cached;
            return;
        }
        
        #if DEBUG
        Log($"CALLED ({seedId})");
        #endif
        
        try
        {
            //if there's no mod data, do by custom fields
            if (ModEntry.Seeds.TryGetValue(seedId, out var mixedSeeds) == false)
            {
#if DEBUG
                Log($"No data in ModSeeds for {seedId}, checking object custom fields");
#endif
                if (Game1.objectData.TryGetValue(seedId, out var objectData) == false || objectData.CustomFields is null)
                    return;

                if (objectData.CustomFields.TryGetValue(ModKeys.MixedSeeds, out var seeds) == false)
                    return;

                //do from custom field
                var splitBySpace = ArgUtility.SplitBySpace(seeds);
                var allFields = new List<string>();

                foreach (var id in splitBySpace)
                {
                    if (id.StartsWith('$') == false)
                    {
                        if (Game1.cropData.TryGetValue(id, out var cropData) == false)
                        {
#if DEBUG
                            Log($"No crop data found. ({id})", LogLevel.Warn);
#endif
                            continue;
                        }
                    
                        //if not in season, no Anytime mod, and outdoors NOT island
                        if(cropData.Seasons.Contains(Game1.season) == false && HasCropsAnytime == false && location.IsOutdoors && location.InIslandContext() == false)
                            continue;
                    }
#if DEBUG
                    Log($"Adding seed id {id}");
#endif
                    switch (id)
                    {
                        case "$vanilla_flowers":
                            allFields.AddRange(GetVanillaFlowersForSeason(Game1.season, location.IsOutdoors));
                            break;
                        case "$vanilla_crops":
                            allFields.AddRange(GetVanillaCropsForSeason(Game1.season, location));
                            break;
                        case "$vanilla_seeds":
                            allFields.AddRange(GetVanillaFlowersForSeason(Game1.season, location.IsOutdoors));
                            allFields.AddRange(GetVanillaCropsForSeason(Game1.season, location));
                            break;
                        default:
                            allFields.Add(id);
                            break;
                    }
                }

                //also add the "main" seed
                for (var i = 0; i < AddMainSeedBy(seedId, allFields); i++)
                {
                    allFields.Add(seedId);
                }

                var fromField = Game1.random.ChooseFrom(allFields);

                Log($"Choosing seed {fromField}");
                Cached = fromField;
                seedId = fromField;

                return;
            }

            var all = new List<string>();

            //if mod data was found
            foreach (var seedData in mixedSeeds)
            {
#if DEBUG
                Log($"Checking seed id {seedData.ItemId}");
#endif
                if (!string.IsNullOrWhiteSpace(seedData.Condition) &&
                    GameStateQuery.CheckConditions(seedData.Condition, location, Game1.player) == false)
                {
#if DEBUG
                    Log($"Conditions don't match. ({seedData.ItemId})");
#endif
                    continue;
                }
                
                //if not found:
                if (seedData.ItemId.StartsWith('$') == false)
                {
                    if (Game1.cropData.TryGetValue(seedData.ItemId, out var cropData) == false)
                    {
#if DEBUG
                        Log($"No crop data found. ({seedData.ItemId})", LogLevel.Warn);
#endif
                        continue;
                    }
                    
                    //if not in season, no Anytime mod, and outdoors NOT island
                    if(cropData.Seasons.Contains(Game1.season) == false && HasCropsAnytime == false && location.IsOutdoors && location.InIslandContext() == false)
                        continue;
                }
#if DEBUG
                Log($"Adding seed id {seedData.ItemId} by {seedData.Weight}", LogLevel.Trace);
#endif
                //add as many times as weight. e.g, weight 1 gets added once
                for (var i = 0; i < seedData.Weight; i++)
                {
                    switch (seedData.ItemId)
                    {
                        case "$vanilla_flowers":
                            all.AddRange(GetVanillaFlowersForSeason(Game1.season, location.IsOutdoors));
                            break;
                        case "$vanilla_crops":
                            all.AddRange(GetVanillaCropsForSeason(Game1.season, location));
                            break;
                        case "$vanilla_seeds":
                            all.AddRange(GetVanillaFlowersForSeason(Game1.season, location.IsOutdoors));
                            all.AddRange(GetVanillaCropsForSeason(Game1.season, location));
                            break;
                        default:
                            all.Add(seedData.ItemId);
                            break;
                    }
                }
            }

            //also add the "main" seed
            for (var i = 0; i < AddMainSeedBy(seedId, all); i++)
            {
                all.Add(seedId);
            }

            //if none in all, return. shouldn't happen but just in case
            if (all.Any() == false)
                return;


            if (seedId == "MixedFlowerSeeds")
            {
                all.AddRange(GetVanillaFlowersForSeason(location.GetSeason(), location.IsOutdoors));
            }
            else if (seedId == "770")
            {
                all.AddRange(GetVanillaCropsForSeason(location.GetSeason(), location));
            }

#if DEBUG
            var allData = new StringBuilder();
            foreach (var str in all)
            {
                allData.Append(str);
                allData.Append(", ");
            }
            Log($"All: {allData}");
#endif
            //if there's none in Add, fallback to random crop (shouldn't happen but still)
            if (all.Count <= 0 || all.Any() == false)
            {
                all.Add(Crop.getRandomLowGradeCropForThisSeason(location.GetSeason()));
            }
            
            var chosen = Game1.random.ChooseFrom(all);
            Log($"Choosing seed {chosen}");
            Cached = chosen;
            seedId = chosen;
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static string[] GetVanillaFlowersForSeason(Season season, bool outdoors)
    {
        if (season == Season.Winter || outdoors == false)
            season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
        return season switch
        {
            Season.Spring => new[] { "427", "429" },
            Season.Summer => new[] { "455", "453", "431" },
            Season.Fall => new[] { "431", "425" },
            _ => Array.Empty<string>()
        };
    }

    private static string[] GetVanillaCropsForSeason(Season season, GameLocation location)
    {
        if (location is IslandLocation)
        {
            return new[] { "479", "833", "481", "478" };
        }
        
        if (season == Season.Winter || location.IsOutdoors == false)
            season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
        
        return season switch
        {
            Season.Spring => new[] { "472", "474", "475", "476" },
            Season.Summer => new[] { "487", "483", "482", "484" },
            Season.Fall => new[] { "487", "488", "489", "490", "491" },
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