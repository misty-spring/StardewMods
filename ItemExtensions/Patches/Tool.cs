using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Patches;

public class ToolPatches
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ToolPatches)}\": postfixing SDV method \"Tool.DoFunction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)),
            postfix: new HarmonyMethod(typeof(ToolPatches), nameof(Post_DoFunction))
        );
    }

    internal static void Post_DoFunction(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
    {
        var furniture = location.GetFurnitureAt(new Vector2(x / 64, y / 64));

        if (furniture is null)
            return;
        
        Log("Furniture found");
        
        if (ModEntry.Resources.TryGetValue(furniture.ItemId, out var resourceData) == false)
            return;
        
        Log("Patching...");

        FurniturePatches.PerformToolAction(furniture, __instance);
    }
}