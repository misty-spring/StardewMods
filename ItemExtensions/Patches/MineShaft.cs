using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class MineShaftPatches
{
    private static readonly string[] VanillaOres = {"", ""};
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
        //if none are an ore
        if (__instance.Objects.Values.Any(o => VanillaOres.Contains(o.ItemId)) == false)
            return;

        var canApply = GetAllForThisLevel(__instance.mineLevel);
        
    }

    private static List<string> GetAllForThisLevel(int mineLevel)
    {
        var all = new List<string>();
        foreach (var (id, ore) in ModEntry.Ores)
        {
            foreach (var VARIABLE in ore.)
            {
                
            }
        }
        return all;
    }
}