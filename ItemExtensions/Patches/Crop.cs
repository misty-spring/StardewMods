using ItemExtensions.Additions;
using ItemExtensions.Models.Contained;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace ItemExtensions.Patches;

internal static class CropPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif

    internal static string Cached { get; set; }
    internal static bool Chosen { get; set; }
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static bool HasCropsAnytime { get; set; }
    
    public static string ResolveSeedId(string itemId, GameLocation location)
    {
#if DEBUG
        Log($"Checking {itemId} data", LogLevel.Warn);
#endif
        if (string.IsNullOrWhiteSpace(itemId))
            return itemId;

        if (Chosen)
        {
#if DEBUG
            Log("Returning cached seed...");
#endif
            return Cached;
        }
        
        Log($"Checking seed {itemId}...");
        
        try
        {
            //if there's mod data
            if (ModEntry.Seeds.TryGetValue(itemId, out var mixedSeeds))
            {
                //get seed and return
                var chosen = GetFromFramework(itemId, mixedSeeds, location);
                Log($"Choosing seed {chosen}");
                Cached = chosen;
                Chosen = true;
                return chosen;
            }

#if DEBUG
            Log($"No data in ModSeeds for {itemId}, checking object's custom fields");
#endif
            if (Game1.objectData.TryGetValue(itemId, out var objectData) == false || objectData.CustomFields is null)
                return itemId;
            
            if (objectData.CustomFields.Any() == false || objectData.CustomFields.TryGetValue(ModKeys.MixedSeeds, out var seeds) == false)
            {
                Log("Found no mixed seeds in custom fields. Using original seed");
                return itemId;
            }

            var fromField = RandomFromCustomFields(itemId, seeds, location);

            Log($"Choosing seed {fromField}");
            Cached = fromField;
            Chosen = true;

            return fromField;
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }

        Chosen = false;
        return itemId;
    }

    private static string GetFromFramework(string seedId, List<MixedSeedData> mixedSeeds, GameLocation location)
    {
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

            //if not a $key, check that crop exists
            if (seedData.ItemId.StartsWith('$') == false)
            {
                //if not found, skip
                if (Game1.cropData.TryGetValue(seedData.ItemId, out var cropData) == false)
                {
                    Log($"No crop data found. ({seedData.ItemId})", LogLevel.Warn);
                    continue;
                }
                
                //if not in season, no Anytime mod, and outdoors NOT island
                if (cropData.Seasons.Contains(Game1.season) == false && HasCropsAnytime == false && location.IsOutdoors && location.InIslandContext() == false)
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

        //if none in all, return original. shouldn't happen but just in case
        if (all.Any() == false)
            return seedId;

        /* this was overriding AddMainSeedBy behavior
        if (all.Any() == false && seedId == "MixedFlowerSeeds")
        {
            all.AddRange(GetVanillaFlowersForSeason(location.GetSeason(), location.IsOutdoors));
        }
        else if (all.Any() == false && seedId == "770")
        {
            all.AddRange(GetVanillaCropsForSeason(location.GetSeason(), location));
        }*/

#if DEBUG
            var allData = new StringBuilder();
            foreach (var str in all)
            {
                allData.Append(str);
                allData.Append(", ");
            }
            Log($"All: {allData}");
#endif
        //if there's no roster, fallback to random crop (shouldn't happen but still)
        if (all.Count <= 0 || all.Any() == false)
        {
            return seedId;
        }

        var chosen = Game1.random.ChooseFrom(all);
        return chosen;
    }

    private static string RandomFromCustomFields(string seedId, string seeds, GameLocation location)
    {
        //get all seeds
        var splitBySpace = ArgUtility.SplitBySpace(seeds);
        var all = new List<string>();

        //for each seed
        foreach (var id in splitBySpace)
        {
            //if not a $key
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
                if (cropData.Seasons.Contains(Game1.season) == false && HasCropsAnytime == false && location.IsOutdoors && location.InIslandContext() == false)
                    continue;
            }
#if DEBUG
            Log($"Adding seed id {id}");
#endif
            //depending on $key:
            switch (id)
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
                //shouldn't happen, but still here
                default:
                    all.Add(id);
                    break;
            }
        }

        //also add the "main" seed
        for (var i = 0; i < AddMainSeedBy(seedId, all); i++)
        {
            all.Add(seedId);
        }

        return Game1.random.ChooseFrom(all);
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
            Season.Spring => new[] { "472", "474", "475" },
            Season.Summer => new[] { "487", "483", "482", "484" },
            Season.Fall => new[] { "487", "488", "489", "490" },
            _ => Array.Empty<string>()
        };
    }

    private static int AddMainSeedBy(string itemId, List<string> allFields)
    {
        var mixedSeeds = itemId is "770" or "MixedFlowerSeeds";
        
        //if not an object
        if (Game1.objectData.TryGetValue(itemId, out var objData) == false)
            return 0;

        //if no crop data AND not mixed seeds
        if (Game1.cropData.TryGetValue(itemId, out _) == false && !mixedSeeds)
            return 0;

        var fields = objData.CustomFields;
        
        // if empty, return "once"
        if (allFields is null || allFields.Any() == false || fields is null || fields.Any() == false)
            return 1;
        
        // if there's any custom fields
        if (fields.Any())
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