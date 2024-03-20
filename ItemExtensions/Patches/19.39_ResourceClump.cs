using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

public class ResourceClumpPatches
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
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": postfixing SDV method \"ResourceClump.OnAddedToLocation\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(TerrainFeature), nameof(TerrainFeature.OnAddedToLocation)),
            postfix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Post_OnAddedToLocation))
        );
        
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": transpiling SDV method \"ResourceClump.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
            prefix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(ResourceClumpPatches.Pre_performToolAction))
        );
    }
    
    public static void Post_OnAddedToLocation(TerrainFeature __instance, GameLocation location, Vector2 tile)
    {
        if (__instance is not ResourceClump r)
            return;
        
        if (r.modData.TryGetValue(ModKeys.CustomClumpId, out var id) is false) 
            return;
        
        if (ModEntry.BigClumps.TryGetValue(id, out var data) == false)
        {
            Log("Clump not found.");
            return;
        }

        var light = data.Light;

        if (light is null || light == new LightData())
            return;

        var fixedPosition = new Vector2(tile.X + r.width.Value / 2, tile.Y * r.height.Value / 2);
        var lightSource = new LightSource(4, fixedPosition, light.Size, light.GetColor());

        r.modData.Add(ModKeys.LightSourceId, $"{lightSource.Identifier}");
    }

    //the transpiler would check if it was ours and return anyway, so let's just prefix
    public static bool Pre_performToolAction(ref ResourceClump __instance, Tool t, int damage, Vector2 tileLocation, ref bool __result)
    {
        if (ExtensionClump.IsCustom(__instance) == false)
        {
            return true;
        }

        __result = ExtensionClump.DoCustom(ref __instance, t, damage, tileLocation);
        return false;
    }
}