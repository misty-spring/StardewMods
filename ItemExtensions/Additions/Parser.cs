using ItemExtensions.Models;
using StardewModdingAPI;

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
            Log($"Checking {item.Key} ...");
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
            ModEntry.MenuActions = parsed;
    }

    
    internal static void ShopExtension(Dictionary<string, Dictionary<string, List<ExtraTrade>>> shopExtensions)
    {
        ModEntry.Shops = new Dictionary<string, Dictionary<string, List<ExtraTrade>>>();
        
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
            ModEntry.Shops.Add(shop.Key, shopTrades);
        }
        ModEntry.Help.GameContent.InvalidateCache("Data/Shops");
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

    public static void Resources(Dictionary<string, ResourceData> clumps)
    {
        ModEntry.Resources = new Dictionary<string, ResourceData>();
        foreach(var pair in clumps)
        {
            Log($"Checking {pair.Key} data...");

            if(pair.Value.IsValid() == false)
                continue;
            
            //add to items
            ModEntry.Resources.Add(pair.Key, pair.Value);
        }

        ModEntry.Help.GameContent.InvalidateCache("Data/Objects");
        ModEntry.Help.GameContent.InvalidateCache("Data/Furniture");
    }
}