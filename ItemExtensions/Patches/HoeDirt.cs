using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

public class HoeDirtPatches
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
        Log($"Applying Harmony patch \"{nameof(HoeDirtPatches)}\": prefixing SDV method \"HoeDirt.canPlantThisSeedHere\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.canPlantThisSeedHere)),
            prefix: new HarmonyMethod(typeof(HoeDirtPatches), nameof(Pre_canPlantThisSeedHere))
        );
        
        Log($"Applying Harmony patch \"{nameof(HoeDirtPatches)}\": prefixing SDV method \"HoeDirt.plant\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant), new[]{typeof(string), typeof(Farmer), typeof(bool)}),
            prefix: new HarmonyMethod(typeof(HoeDirtPatches), nameof(Pre_plant))
        );
        
        Log($"Applying Harmony patch \"{nameof(HoeDirtPatches)}\": postfixing SDV method \"HoeDirt.plant\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant), new[]{typeof(string), typeof(Farmer), typeof(bool)}),
            postfix: new HarmonyMethod(typeof(HoeDirtPatches), nameof(Post_plant))
        );
    }
    
    private static void Pre_canPlantThisSeedHere(string itemId, bool isFertilizer = false)
    {
#if DEBUG
        Log("Called canPlant from hoedirt", LogLevel.Warn);
#endif
    }
    
    private static void Pre_plant(ref string itemId, Farmer who, bool isFertilizer)
    {
#if DEBUG
        Log("Called plant", LogLevel.Warn);
#endif
        //itemId = CropPatches.ResolveSeedId(itemId, who.currentLocation);
    }
    
    private static void Post_plant(string itemId, Farmer who, bool isFertilizer)
    {
        Log($"Clearing seed cache...(last item {itemId})");
        CropPatches.Cached = null;
    }
}