using ItemExtensions.Additions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace ItemExtensions.Models.Internal;

public static class ExtensionClump
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    public static ResourceClump Create (string id, ResourceData data, Vector2 position, int remainingHealth = -1)
    {
        var clump = new ResourceClump(data.SpriteIndex, data.Width, data.Height, position,
            remainingHealth > 0 ? remainingHealth : data.Health, data.Texture);
        
        clump.modData.Add(ModKeys.ClumpId, id);
        
        if (data.Light is not null)
        {
            clump.modData.Add(ModKeys.LightSize, $"{data.Light.Size}");
            clump.modData.Add(ModKeys.LightColor, $"{data.Light.ColorString()}");
        }
        
        return clump;
    }
    
    public static ResourceClump Create (string id, Vector2 position, int remainingHealth = -1)
    {
        if (ModEntry.BigClumps.TryGetValue(id, out var data) == false)
        {
            Log($"Data for {id} not found. (clumps)");
            return null;
        }
        
        var clump = Create(id, data, position, remainingHealth);
        return clump;
    }

    /// <summary>
    /// Tries to get custom clump ID from mod data. if it exists, uses ID to check if in big clumps.
    /// </summary>
    /// <param name="clump"></param>
    /// <returns></returns>
    public static bool IsCustom(ResourceClump clump)
    {
        if (clump.modData.TryGetValue(ModKeys.ClumpId, out var id) == false)
            return false;
        
        return ModEntry.BigClumps.TryGetValue(id, out _);
    }
    
    /// <summary>
    /// Actions to do on tool action or bomb explosion.
    /// </summary>
    /// <param name="clump"></param>
    /// <param name="t">Tool used</param>
    /// <param name="damage">Damage made</param>
    /// <param name="tileLocation">Location of hit</param>
    /// <returns>If the clump must be destroyed.</returns>
    public static bool DoCustom(ref ResourceClump clump, Tool t, int damage, Vector2 tileLocation)
    {
        if (clump.modData.TryGetValue(ModKeys.ClumpId, out var id) is false) 
            return false;
        
        if (ModEntry.BigClumps.TryGetValue(id, out var resource) == false)
            return false;

        if (!GeneralResource.ToolMatches(t, resource))
            return false;

        //set vars
        int parsedDamage;
        
        if (t is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            parsedDamage = middle / 10;
        }
        else if (t is null)
        {
            parsedDamage = damage;
        }
        else
        {
            parsedDamage = (int)Math.Max(1f, (t.UpgradeLevel + 1) * 0.75f);
        }
        
        #if DEBUG
        Log($"Damage: {parsedDamage}");
        #endif

        if (parsedDamage <= 0)
            return false;
        
        //if health data doesn't exist, idk if it can Not exist but just in case
        try
        {
            _ = clump.health.Value;
        }
        catch(Exception)
        {
            clump.health.Set(resource.Health);
        }

        if (damage < resource.MinToolLevel)
        {
            clump.Location.playSound("clubhit", tileLocation);
            clump.Location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
            Game1.player.jitterStrength = 1f;
            return false;
        }

        if(!string.IsNullOrWhiteSpace(resource.Sound))
            clump.Location.playSound(resource.Sound, tileLocation);
        
        clump.health.Value -= parsedDamage;
        
        #if DEBUG
        Log($"Remaining health {clump.health.Value}");
        #endif

        if (clump.health.Value <= 0.0)
        {
            //create drops & etc
            GeneralResource.CheckDrops(resource, clump.Location, tileLocation, t);

            //if has a light ID, remove
            if (clump.modData.TryGetValue(ModKeys.LightId, out var lightSourceRaw))
            {
                if (int.TryParse(lightSourceRaw, out var lightSource))
                {
                    clump.Location.removeLightSource(lightSource);
                }
            }

            //var location = __instance.Location;
            //location.resourceClumps.Remove(__instance);
            return true;
        }

        if (resource.Shake)
        {
            var shakeTimerReflected = ModEntry.Help.Reflection.GetField<float>(clump, "shakeTimer");
            shakeTimerReflected.SetValue(100);
        }
        
        return false;
    }
}