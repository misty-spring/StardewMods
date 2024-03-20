using HarmonyLib;
using ItemExtensions.Models;
using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Patches;

public class GameLocationPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": postfixing SDV method \"GameLocation.spawnObjects\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnObjects)),
            postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_spawnObjects))
        );
    }

    private static void Post_spawnObjects(GameLocation __instance)
    {
        Log($"Checking spawns made at {__instance.DisplayName ?? __instance.NameOrUniqueName}");
        
        foreach (var item in __instance.Objects.Values)
        {
            if(!ModEntry.Data.TryGetValue(item.QualifiedItemId, out var data))
                continue;
            
            if(data.Resource is null || data.Resource == new ResourceData())
                continue;
            
            Log($"Setting spawn data for {item.DisplayName}");
            
            Additions.World.SetSpawnData(item, data.Resource);
        }
    }
}