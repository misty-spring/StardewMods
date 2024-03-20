using HarmonyLib;
using ItemExtensions.Additions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

public class TreePatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": postfixing SDV method \"Tree.performUseAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), "performUseAction"),
            postfix: new HarmonyMethod(typeof(TreePatches), nameof(Post_performUseAction))
        );
        
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": prefixing SDV method \"Tree.performTreeFall\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), "performTreeFall"),
            prefix: new HarmonyMethod(typeof(TreePatches), nameof(Pre_performTreeFall))
        );
        
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": prefixing SDV method \"FruitTree.shake\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.shake)),
            prefix: new HarmonyMethod(typeof(TreePatches), nameof(Pre_shake))
        );
        
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": postfixing SDV method \"FruitTree.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performToolAction)),
            postfix: new HarmonyMethod(typeof(TreePatches), nameof(Post_performToolAction))
        );
    }
    
    private static void Post_performUseAction(Tree __instance, Vector2 tileLocation, bool __result)
    {
        try
        {
            //if not shakeable
            if (__result == false)
                return;

            //if no data
            if (ModEntry.Trees.TryGetValue(__instance.treeType.Value, out var data) == false)
                return;

            //if no fall data
            if (data.OnShake is null || data.OnShake.Any() == false)
                return;

            GeneralResource.TryExtraDrops(data.OnShake, __instance.Location, Game1.player, tileLocation);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
    
    private static void Pre_performTreeFall(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        try
        {
            //if no data
            if (ModEntry.Trees.TryGetValue(__instance.treeType.Value, out var data) == false)
                return;

            //if no fall data
            if (data.OnFall is null || data.OnFall.Any() == false)
                return;

            GeneralResource.TryExtraDrops(data.OnFall, __instance.Location, t.getLastFarmerToUse(), tileLocation);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static void Pre_shake(FruitTree __instance, Vector2 tileLocation, bool doEvenIfStillShaking)
    {
        try
        {
            //if no fruit
            if (__instance.fruit.Count <= 0)
                return;

            //if no data
            if (ModEntry.Trees.TryGetValue(__instance.treeId.Value, out var data) == false)
                return;

            //if no shake data
            if (data.OnShake is null || data.OnShake.Any() == false)
                return;

            GeneralResource.TryExtraDrops(data.OnShake, __instance.Location, Game1.player, tileLocation);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static void Post_performToolAction(FruitTree __instance, Tool t, int explosion, Vector2 tileLocation,
        bool __result)
    {
        try
        {
            //if not destroyed
            if (__result == false)
                return;

            //if no data
            if (ModEntry.Trees.TryGetValue(__instance.treeId.Value, out var data) == false)
                return;

            //if no fall data
            if (data.OnFall is null || data.OnFall.Any() == false)
                return;

            GeneralResource.TryExtraDrops(data.OnFall, __instance.Location, t.getLastFarmerToUse(), tileLocation);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
}