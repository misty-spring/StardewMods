using ItemExtensions.Additions;
using ItemExtensions.Models.Internal;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Events;

public class Day
{
    public static void Started(object sender, DayStartedEventArgs e)
    {
        List<ResourceClump> clumps = new();
        
        Utility.ForEachLocation(LoadCustomClumps, true, true);

        foreach (var resourceClump in clumps)
        {
            //copy data
            var position = resourceClump.Tile * 64;
            var health = resourceClump.health.Value;
            var texture = resourceClump.textureName.Value;
            var id = resourceClump.modData[ModKeys.CustomClumpId];
            
            //remove
            var location = resourceClump.Location;
            location.resourceClumps.Remove(resourceClump);
            
            //get data
            if(ModEntry.BigClumps.TryGetValue(id, out var data) == false)
                return;
            
            //make replacement
            var replacement = new ExtensionClump(id, data, position, (int)health);

            //add
            location.resourceClumps.Add(replacement);
        }

        return;
        
        bool LoadCustomClumps(GameLocation arg)
        {
            var newClumps = new List<ResourceClump>();
            
            foreach (var resource in arg.resourceClumps)
            {
                if(resource.modData.TryGetValue(ModKeys.IsCustomClump, out var raw) == false)
                    continue;

                //if not a custom clump
                if(bool.TryParse(raw, out var isCustom) == false || isCustom == false)
                    continue;
                    
                newClumps.Add(resource);
            }

            return newClumps.Any();
        }
    }

    public static void Ending(object sender, DayEndingEventArgs e)
    {
        List<ExtensionClump> clumps = new();
        
        Utility.ForEachLocation(CheckForCustomClumps, true, true);

        foreach (var resourceClump in clumps)
        {
            //copy data
            var index = resourceClump.parentSheetIndex.Value;
            var w = resourceClump.width.Value;
            var h = resourceClump.height.Value;
            var position = resourceClump.Tile * 64;
            var health = resourceClump.health.Value;
            var texture = resourceClump.textureName.Value;
            
            //remove
            var location = resourceClump.Location;
            location.resourceClumps.Remove(resourceClump);
            
            //make replacement
            var replacement = new ResourceClump(index, w, h, position, (int)health, texture);
            replacement.modData.Add(ModKeys.IsCustomClump, "true");
            replacement.modData.Add(ModKeys.CustomClumpId, resourceClump.ResourceId);

            //add
            location.resourceClumps.Add(replacement);
        }

        return;

        bool CheckForCustomClumps(GameLocation arg)
        {
            var newClumps = new List<ResourceClump>();
            
            foreach (var resource in arg.resourceClumps)
            {
                if(resource is not ExtensionClump)
                    continue;
                
                /*
                if(resource.modData.TryGetValue(ModKeys.IsCustomClump, out var raw) == false)
                    continue;
                
                //if not a custom clump
                if(bool.TryParse(raw, out var isCustom) == false || isCustom == false)
                    continue;
                */
                    
                newClumps.Add(resource);
            }

            return newClumps.Any();
        }
    }
}