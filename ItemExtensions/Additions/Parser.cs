using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
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
                Log($"Information parsed successfully! ({rightInfo.TargetId})");
#endif
                temp.Add(rightInfo);
            }
            
            if(temp.Count > 0)
                parsed.Add(item.Key,temp);
        }

        if (parsed.Count > 0)
            ModEntry.MenuActions = parsed;
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
            if(anim.Key.StartsWith("base") == false)
                Log($"Checking {anim.Key} data...");
            
            if(!anim.Value.IsValid(out var parsed))
                continue;
            
            //add to items
            ModEntry.EatingAnimations.Add(anim.Key, parsed);
        }
    }

    public static void Resources(Dictionary<string, ResourceData> clumps, bool skipTextureCheck = false)
    {
        ModEntry.Ores = new Dictionary<string, ResourceData>();
        ModEntry.BigClumps = new Dictionary<string, ResourceData>();
        foreach(var (id, data) in clumps)
        {
            if(string.IsNullOrWhiteSpace(id))
                continue;
            
            Log($"Checking {id} data...");

            if(data.IsValid(skipTextureCheck) == false)
                continue;

            //check it's not vanilla
            if (int.TryParse(id, out var asInt))
            {
                //if it's a vanilla ID, ignore
                if (asInt < 1000)
                    continue;
                
                //if vanilla resource, trim
                if (GeneralResource.VanillaIds.Contains(asInt))
                    data.Trim(asInt);
            }
            
            //add depending on size
            if(data.Width > 1 || data.Height > 1)
                ModEntry.BigClumps.Add(id, data);
            else
                ModEntry.Ores.Add(id, data);
        }

        ModEntry.Help.GameContent.InvalidateCache("Data/Objects");
    }

    public static void MixedSeeds(Dictionary<string, List<MixedSeedData>> seeds)
    {
        ModEntry.Seeds = new Dictionary<string, List<MixedSeedData>>();
        foreach(var pair in seeds)
        {
            Log($"Checking {pair.Key} data...", LogLevel.Debug);

            var validSeeds = new List<MixedSeedData>();
            var hasAllSeeds = true;
            
            foreach (var data in pair.Value)
            {
                //checks id
                if (data.IsValid())
                {
                    //checks sub conditions like having a mod and season
                    if (data.CheckConditions())
                    {
                        validSeeds.Add(data);
                    }
                    
                    continue;
                }
                
                hasAllSeeds = false;
                break;
            }
    
            //add depending on size
            if(hasAllSeeds == false)
                continue;
            else
                ModEntry.Seeds.Add(pair.Key, validSeeds);
        }
    }
}