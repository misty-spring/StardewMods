using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ItemExtensions.Patches;

public class UtilityPatches
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
        Log($"Applying harmony patch {nameof(UtilityPatches)}: postfixing SDV method Utility.canGrabSomethingFromHere(int, int, Farmer)");
        harmony.Patch(original: AccessTools.Method(typeof(Utility), nameof(Utility.canGrabSomethingFromHere), new[] { typeof(int), typeof(int), typeof(Farmer) }),
            postfix: new HarmonyMethod(typeof(UtilityPatches), nameof(Post_canGrabSomethingFromHere)));
    }

    public static void Post_canGrabSomethingFromHere(int x, int y, Farmer who, ref bool __result)
    {
        if (__result == false)
            return;

        if (Game1.mouseCursor <= 0) 
            return;

        if (who?.IsLocalPlayer != true || !Context.IsPlayerFree || who.currentLocation is null) 
            return;
        
        var objectAt = who.currentLocation.getObjectAt(x, y);
        if (objectAt is not Furniture f)
            return;

        if (ModEntry.Ores.ContainsKey(f.ItemId) == false)
            return;

        Game1.mouseCursor = 0;

        if (!Utility.withinRadiusOfPlayer(x, y, 1, who)) 
            return;
        
        __result = false;
        Game1.mouseCursorTransparency = 0.5f;
    }
}