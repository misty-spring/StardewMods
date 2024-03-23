using System.Text;
using HarmonyLib;
using ItemExtensions.Additions.Clumps;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class MineShaftPatches
{
    private static readonly string[] VanillaStones =
    {
        //copper (751) and iron (290) are fairly low-cost, so they're replaced by default. but because gold and iridium are rarer, they're excluded. the rest of IDs are stones
        "32", "34", "36", "38", "40", "42", "48", "50", "52", "54", "56", "58", "290", "450", "668", "670", "751", "760", "762"
    };
    internal static List<string> OrderedByChance { get; set; }= new();
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(MineShaftPatches)}\": postfixing SDV method \"MineShaft.populateLevel()\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
            postfix: new HarmonyMethod(typeof(MineShaftPatches), nameof(Post_populateLevel))
        );
    }
    
    private static void Post_populateLevel(MineShaft __instance)
    {
        try
        {
            //don't patch anything that's negative
            if (__instance.mineLevel < 1) //|| __instance.mineLevel % 10 == 0 //possible "reward" levels don't need a check because they won't have stones anyway
                return;

            CheckResourceNodes(__instance);
            
            //clumps aren't changed here to avoid issues because the zone is special
            if(__instance.mineLevel != 77377)
                CheckResourceClumps(__instance);
        }
        catch (Exception e)
        {
            Log($"Error when postfixing populate for level {__instance.mineLevel}: {e}", LogLevel.Error);
        }
    }

    private static void CheckResourceNodes(MineShaft mineShaft)
    {
        //if none are an ore
        if (mineShaft.Objects.Values.Any(o => VanillaStones.Contains(o.ItemId)) == false)
            return;

        //randomly chooses which stones to replace
        var all = mineShaft.Objects.Values.Where(o => VanillaStones.Contains(o.ItemId));

        var canApply = GetAllForThisLevel(mineShaft.mineLevel);
        if (canApply is null || canApply.Any() == false)
            return;

        //for every stone we selected
        foreach (var stone in all)
        {
            //choose a %
            var nextDouble = Game1.random.NextDouble();
            foreach (var (id, chance) in canApply)
            {
                //if % isn't caught by this ore, try with next one
                if (nextDouble > chance)
                    continue;

                //create data for fixed ore
                var ore = new Object(id, 1)
                {
                    TileLocation = stone.TileLocation, 
                    //Location = stone.Location,
                    MinutesUntilReady = ModEntry.Ores[id].Health
                };

                //replace & break to avoid re-setting
                mineShaft.Objects[stone.TileLocation] = ore;
                break;
            }
        }
    }

    private static void CheckResourceClumps(MineShaft mineShaft)
    {
        //if none are a clump
        if (mineShaft.terrainFeatures.Values.Any(t => t is ResourceClump == false))
            return;

        //randomly chooses which stones to replace
        var all = mineShaft.terrainFeatures.Values.Where(t => t is ResourceClump);

        var canApply = GetAllForThisLevel(mineShaft.mineLevel, true);
        if (canApply is null || canApply.Any() == false)
            return;

        //for every stone we selected
        foreach (var stone in all)
        {
            //choose a %
            var nextDouble = Game1.random.NextDouble();
            foreach (var (id, chance) in canApply)
            {
                //if % isn't caught by this ore, try with next one
                if (nextDouble > chance)
                    continue;

                //create data for fixed ore
                var newClump = ExtensionClump.Create(id, ModEntry.BigClumps[id], stone.Tile);

                //replace & break to avoid re-setting
                mineShaft.terrainFeatures[stone.Tile] = newClump;
                break;
            }
        }
    }

    private static Dictionary<string, double> GetAllForThisLevel(int mineLevel, bool isClump = false)
    {
        var all = new Dictionary<string, double>();
        //check every ore
        foreach (var (id, ore) in isClump ? ModEntry.BigClumps : ModEntry.Ores)
        {
            //if not spawnable on mines, skip
            if(ore.SpawnableFloors is null || ore.SpawnableFloors.Any() == false)
                continue;

            var extraforLevel = ore.AdditionalChancePerLevel * mineLevel;
                
            foreach (var floor in ore.SpawnableFloors)
            {
                //if it's of style minSpawnLevel-maxSpawnLevel
                if (floor.Contains('-'))
                {
                    var both = floor.Split('-');
                    //if less than 2 values, initial is bigger than current OR max is less than current
                    if (both.Length < 2 || int.Parse(both[0]) > mineLevel || int.Parse(both[1]) < mineLevel)
                        break; //skip
                    
                    //otherwise, add & break loop
                    all.Add(id, ore.SpawnFrequency + extraforLevel);
                    break;
                }
                
                //or if level is explicitly included
                if(int.TryParse(floor, out var isInt) && isInt == mineLevel)
                    all.Add(id, ore.SpawnFrequency  + extraforLevel);
            }
        }
        var sorted = from entry in all orderby entry.Value select entry;
        
        var result = new Dictionary<string, double>();
        foreach (var pair in sorted)
        {
            result.Add(pair.Key, pair.Value);
        }

        return result;
        
        #if DEBUG
        var sb = new StringBuilder();
        foreach (var pair in result)
        {
            sb.Append("{ ");
            sb.Append(pair.Key);
            sb.Append(", ");
            sb.Append(pair.Value);
            sb.Append(" }");
            sb.Append(", ");
        }
        Log($"In level {mineLevel}: " + sb);
        #endif
        return result;
    }
}