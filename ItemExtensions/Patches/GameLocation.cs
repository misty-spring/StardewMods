using HarmonyLib;
using ItemExtensions.Events;
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
            if(!ModEntry.Resources.TryGetValue(item.ItemId, out var resource))
                continue;
            
            if(resource is null || resource == new ResourceData())
                continue;
            
            Log($"Setting spawn data for {item.DisplayName}");
            
            World.SetSpawnData(item, resource);
        }
    }
}