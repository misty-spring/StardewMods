using System.Text;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class MineShaftPatches
{
    private static readonly string[] VanillaStones = {"", ""};
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
            if (__instance.mineLevel < 1)
                return;
            
            //if none are an ore
            if (__instance.Objects.Values.Any(o => VanillaStones.Contains(o.ItemId)) == false)
                return;

            //randomly chooses which stones to replace
            var all = __instance.Objects.Values.Where(o => VanillaStones.Contains(o.ItemId));

            var canApply = GetAllForThisLevel(__instance.mineLevel);
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
                        TileLocation = stone.TileLocation, Location = stone.Location,
                        MinutesUntilReady = ModEntry.Ores[id].Health
                    };

                    //replace & break to avoid re-setting
                    __instance.Objects[stone.TileLocation] = ore;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Log($"Error when postfixing populate for level {__instance.mineLevel}: {e}", LogLevel.Error);
        }
    }

    private static Dictionary<string, double> GetAllForThisLevel(int mineLevel)
    {
        var all = new Dictionary<string, double>();
        //check every ore
        foreach (var (id, ore) in ModEntry.Ores)
        {
            //if not spawnable on mines, skip
            if(ore.SpawnableFloors is null || ore.SpawnableFloors.Any() == false)
                continue;
            
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
                    all.Add(id, ore.SpawnFrequency);
                    break;
                }
                
                //or if level is explicitly included
                if(int.TryParse(floor, out var isInt) && isInt == mineLevel)
                    all.Add(id, ore.SpawnFrequency);
            }
        }
        var sorted = from entry in all orderby entry.Value select entry;
        
        var result = (Dictionary<string, double>)sorted.AsEnumerable();
        
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