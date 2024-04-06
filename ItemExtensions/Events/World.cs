using ItemExtensions.Additions;
using ItemExtensions.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ItemExtensions.Events;

public static class World
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    public static void ObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        if(!e.Added.Any())
            return;

        foreach (var pair in e.Added)
        {
            if (ModEntry.Ores.TryGetValue(pair.Value.ItemId, out var resource) == false)
                continue;

            if (resource is null)
                continue;
            
            if(GeneralResource.IsVanilla(pair.Value.ItemId))
                continue;
            
            Log("Found data...");

            SetSpawnData(pair.Value, resource);
        }
    }

    internal static void SetSpawnData(Object o, ResourceData resource)
    {
        o.MinutesUntilReady = resource.Health;
        
        if (o.tempData is null)
        {
            o.tempData =  new Dictionary<string, object>
            {
                { "Health", resource.Health }
            };
        }
        else
        {
            o.tempData.TryAdd("Health", resource.Health);
        }

        o.modData["Esca.FarmTypeManager/CanBePickedUp"] = "false";
        
        if(o.tempData.ContainsKey(ModKeys.IsFtm) == false)
            o.modData[ModKeys.Days] = "0";
        
        o.CanBeSetDown = true;
        o.CanBeGrabbed = false;
        o.IsSpawnedObject = false;
        
        o.initializeLightSource(o.TileLocation);
    }

    public static void TerrainChanged(object sender, TerrainFeatureListChangedEventArgs e)
    {
#if DEBUG
        if (e.Removed.Any())
        {
            foreach (var tf in e.Removed)
            {
                Log("Removed at " + tf.Value.Tile);
            }
        }
#endif
        if(!e.Added.Any())
            return;
#if DEBUG
        Log($"Checking added terrain features", LogLevel.Info);
#endif
        
        foreach (var pair in e.Added)
        {
            if (pair.Value is not ResourceClump clump)
            {
#if DEBUG
                Log($"Terrain feature is of type {pair.Value.GetType()}. Skipping");
#endif
                continue;
            }
#if DEBUG
            Log($"Checking added clump {clump?.modData?[ModKeys.ClumpId] ?? "[no id]"}");
#endif
            
            if(clump.modData is null || clump.modData.ContainsKey(ModKeys.ClumpId))
                continue;
            
            if (TryGetClumpData(clump, out var id, out _) == false)
                continue;
            
            Log("Found data for clump...");

            clump.modData.Add(ModKeys.ClumpId, id);
        }
    }

    internal static bool TryGetClumpData(ResourceClump clump, out string id, out ResourceData data)
    {
        foreach (var (entry, resourceData) in ModEntry.BigClumps)
        {
            if(resourceData.Texture != clump.textureName.Value)
                continue;

            if (resourceData.SpriteIndex != clump.parentSheetIndex.Value)
                continue;

            if (resourceData.Width != clump.width.Value)
                continue;
            
            if (resourceData.Height != clump.height.Value)
                continue;
#if DEBUG
            Log($"Found ID {entry} for clump!");
#endif
            id = entry;
            data = resourceData;
            return true;
        }

        id = null;
        data = null;
        return false;
    }
}