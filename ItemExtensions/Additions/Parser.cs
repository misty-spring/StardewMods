using ItemExtensions.Models;
using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Additions;

public static class Parser
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void ItemActions(Dictionary<string, List<MenuBehavior>> ia)
    {
        var parsed = new Dictionary<string, List<MenuBehavior>>();
        foreach (var item in ia)
        {
            Log($"Checking {item.Key} ...", LogLevel.Debug);
            var temp = new List<MenuBehavior>();
            
            foreach (var affected in item.Value)
            {
                if (!affected.Parse(out var rightInfo)) 
                    continue;
#if DEBUG
                Log($"Information parsed successfully! ({rightInfo.TargetID})");
#endif
                temp.Add(rightInfo);
            }
            
            if(temp.Count > 0)
                parsed.Add(item.Key,temp);
        }

        if (parsed.Count > 0)
            ModEntry.ItemActions = parsed;
    }

    internal static void ShopExtension(Dictionary<string, Dictionary<string, List<ExtraTrade>>> shopExtensions)
    {
        ModEntry.ExtraTrades = new Dictionary<string, Dictionary<string, List<ExtraTrade>>>();
        
        foreach(var shop in shopExtensions)
        {
            Log($"Checking {shop.Key} data...");
            
            var shopTrades = new Dictionary<string,List<ExtraTrade>>();
            foreach (var (id, rawData) in shop.Value)
            {
                Log($"Checking item {id}...");
                
                var parsedExtras = new List<ExtraTrade>();
                foreach (var singleExtra in rawData)
                {
                    //Mon.Log($"Checking extra trade {singleExtra.QualifiedItemId}...", Level);
                    
                    if(singleExtra.IsValid(out var rightData) == false)
                    {
                        continue;
                    }
                    
                    parsedExtras.Add(rightData);
                }
                
                //if no extras are valid
                if(parsedExtras.Count <= 0)
                    continue;
                
                //add item and parsed data
                shopTrades.Add(id, parsedExtras);
            }
            
            //if no trades were valid
            if(shopTrades.Count <= 0)
                continue;
            
            //add to extra trades
            ModEntry.ExtraTrades.Add(shop.Key, shopTrades);
        }
    }

    internal static void ObjectData(Dictionary<string, ItemData> objData)
    {
        ModEntry.Data = new Dictionary<string, ItemData>();
        foreach(var obj in objData)
        {
            Log($"Checking {obj.Key} data...");

            var light = obj.Value.Light;
            if (light != null)
            {
                if (light.Size == 0)
                {
                    Log($"Item light can't be 0. Skipping {obj.Key}.", LogLevel.Warn);
                    continue;
                }
                
                if(light.Transparency == 0)
                {
                    Log($"Item transparency can't be 0. Skipping {obj.Key}.", LogLevel.Warn);
                    continue;
                }
            }
            
            var resource = obj.Value.Resource;
            if (resource != null)
            {
                if (resource.Health <= 0)
                {
                    Log($"Resource health must be over 0. Skipping {obj.Key}.", LogLevel.Warn);
                    continue;
                }
                
                if(string.IsNullOrWhiteSpace(resource.Tool))
                {
                    Log($"Must specify a tool for resource. Skipping {obj.Key}.", LogLevel.Warn);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(resource.ItemDropped))
                {
                    Log($"Resource's dropped item is empty. ({obj.Key})", LogLevel.Warn);
                    Log($"The item will still be added, but this may cause issues.");
                }
            }
            
            //add to items
            ModEntry.Data.Add(obj.Key, obj.Value);
            
            #if DEBUG
            Log("Added successfully.");
            #endif
        }
    }

    public static void EatingAnimations(Dictionary<string, FarmerAnimation> animations)
    {
        ModEntry.EatingAnimations = new Dictionary<string, FarmerAnimation>();
        foreach(var anim in animations)
        {
            Log($"Checking {anim.Key} data...");

            if(!anim.Value.IsValid(out var parsed))
                continue;
            
            //add to items
            ModEntry.EatingAnimations.Add(anim.Key, parsed);
        }
    }
}