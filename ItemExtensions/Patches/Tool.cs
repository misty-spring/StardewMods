using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace ItemExtensions.Patches;

public class ToolPatches
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
            original: AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)),
            postfix: new HarmonyMethod(typeof(ToolPatches), nameof(Post_DoFunction))
        );
    }

    private static void Post_DoFunction(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
    {
        try
        {
            if (__instance is not Pickaxe)
                return;

            if (location.isObjectAtTile(x, y) == false)
                return;

            location.getObjectAtTile(x, y)?.performToolAction(__instance);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
}