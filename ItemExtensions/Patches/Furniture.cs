using HarmonyLib;
using ItemExtensions.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using static ItemExtensions.Patches.ObjectPatches;

namespace ItemExtensions.Patches;

public class FurniturePatches
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
        /*
        Log($"Applying Harmony patch \"{nameof(FurniturePatches)}\": postfixing SDV method \"Furniture.initializeLightSource\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Furniture), "initializeLightSource"),
            postfix: new HarmonyMethod(typeof(FurniturePatches), nameof(Post_initializeLightSource))
        );*/
        
        Log($"Applying Harmony patch \"{nameof(FurniturePatches)}\": prefixing SDV method \"Furniture.actionOnPlayerEntryOrPlacement\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Furniture), nameof(Furniture.actionOnPlayerEntryOrPlacement)),
            prefix: new HarmonyMethod(typeof(FurniturePatches), nameof(Post_actionOnPlayerEntryOrPlacement))
        );
    }
    
    private static void Post_actionOnPlayerEntryOrPlacement(Furniture __instance, GameLocation environment, bool dropDown)
    {
        if (ModEntry.Resources.TryGetValue(__instance.ItemId, out var resData) == false)
            return;
        
        World.SetSpawnData(__instance, resData);

        __instance.CanBeGrabbed = false;
        
        var x = (int)__instance.TileLocation.X + __instance.getTilesWide() / 2;
        var y = (int)__instance.TileLocation.Y + __instance.getTilesHigh() / 2;
        __instance.lightSource?.position.Set(x * 64f, y * 64f);
    }

    internal static void PerformToolAction(Furniture f, Tool t)
    {
        if (ModEntry.Resources.TryGetValue(f.ItemId, out var resource) == false)
            return;

        if (!ToolMatches(t, resource))
            return;

        //set vars
        var location = f.Location;
        var tileLocation = f.TileLocation;
        var damage = GetDamage(t);
        
        //if temp data doesn't exist, it means we also have to set minutes until ready
        try
        {
            _ = f.tempData["Health"];
        }
        catch(Exception)
        {
            f.tempData ??= new Dictionary<string, object>();
            f.tempData.TryAdd("Health", resource.Health);
            f.MinutesUntilReady = resource.Health;
        }

        if (damage < resource.MinToolLevel)
        {
            location.playSound("clubhit", tileLocation);
            location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
            Game1.player.jitterStrength = 1f;
            return;
        }

        if(!string.IsNullOrWhiteSpace(resource.Sound))
            location.playSound(resource.Sound, tileLocation);
        
        /*var num1 = t.UpgradeLevel + 1; //Math.Max(1f, (t.UpgradeLevel + 1) * 0.75f);
        health -= num1;
        //re-set health info
        __instance.tempData["Health"] = health;*/
        f.MinutesUntilReady -= damage + 1;

        if (f.MinutesUntilReady <= 0.0)
        {
            //create notes
            DoResourceDrop(f, t, resource);

            Destroy(f);
            
            if(resource.ActualSkill >= 0)
                t.getLastFarmerToUse().gainExperience(resource.ActualSkill, resource.Exp);
            
            return;
        }
        f.shakeTimer = 100;
    }
    
    private static void Destroy(Furniture f, bool onlySetDestroyable = false)
    {
        f.CanBeSetDown = true;
        f.CanBeGrabbed = true;
        f.IsSpawnedObject = true; //by default false IIRC
        
        if(onlySetDestroyable)
            return;
        
        //o.performRemoveAction();
        var location = f.Location;
        location.furniture.Remove(f);
        //f.Location.removeObject(f.TileLocation,false);
        f = null;
    }
}