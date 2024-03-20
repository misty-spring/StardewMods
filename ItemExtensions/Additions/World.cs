using ItemExtensions.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Object = StardewValley.Object;

namespace ItemExtensions.Additions;

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
            if (!ModEntry.Data.TryGetValue(pair.Value.ItemId, out var mainData))
                continue;
            
            Log("Found data...");
            
            if(mainData.Resource == null || mainData.Resource == new ResourceData())
                return;

            SetSpawnData(pair.Value, mainData.Resource);
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
        o.CanBeSetDown = true;
        o.CanBeGrabbed = false;
        o.IsSpawnedObject = false;
    }
}