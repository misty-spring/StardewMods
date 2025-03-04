using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Items;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ItemExtensions.Events;

public class Save
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    private static IModHelper Help => ModEntry.Help;
    private static ModConfig Config => ModEntry.Config;
    private static string Id => ModEntry.Id;
    
    /// <summary>
    /// At this point, the mod loads its files and adds contentpacks' changes. It also loads custom clump texture for the first time.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal static void OnLoad(object sender, SaveLoadedEventArgs e)
    {
        //get obj data
        var objData = Help.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Data");
        Parser.ObjectData(objData);
        Log($"Loaded {ModEntry.Data?.Count ?? 0} item data.", LogLevel.Debug);
        
        if (Config.EatingAnimations)
        {
            //get custom animations
            var animations = Help.GameContent.Load<Dictionary<string, FarmerAnimation>>($"Mods/{Id}/EatingAnimations");
            Parser.EatingAnimations(animations);
            Log($"Loaded {ModEntry.EatingAnimations?.Count ?? 0} eating animations.", LogLevel.Debug);
        }
        
        if (Config.Resources)
        {
            //get extra terrain for mineshaft
            var trees = Help.GameContent.Load<Dictionary<string, TerrainSpawnData>>($"Mods/{Id}/Mines/Terrain");
            Parser.Terrain(trees);
            Log($"Loaded {ModEntry.MineTerrain?.Count ?? 0} mineshaft terrain features.", LogLevel.Debug);
        }
        
        if (Config.MixedSeeds)
        {
            //get mixed seeds
            var seedData = Help.GameContent.Load<Dictionary<string, List<MixedSeedData>>>($"Mods/{Id}/MixedSeeds");
            Parser.MixedSeeds(seedData);
            Log($"Loaded {ModEntry.Seeds?.Count ?? 0} mixed seeds data.", LogLevel.Debug);
        }
        
        if (Config.Panning)
        {
            //get panning
            var panData = Help.GameContent.Load<Dictionary<string, PanningData>>($"Mods/{Id}/Panning");
            Parser.Panning(panData);
            Log($"Loaded {ModEntry.Panning?.Count ?? 0} panning data.", LogLevel.Debug);
        }
        
        if(Config.TrainDrops)
        {
            //train stuff
            var trainData = Help.GameContent.Load<Dictionary<string, TrainDropData>>($"Mods/{Id}/Train");
            Parser.Train(trainData);
            Log($"Loaded {ModEntry.TrainDrops?.Count ?? 0} custom train drops.", LogLevel.Debug);
        }
        
        if(Config.Treasure)
            ModEntry.Treasure = Help.GameContent.Load<Dictionary<string, TreasureData>>($"Mods/{Id}/Treasure");

        //ACTION BUTTON LIST
        var temp = new List<SButton>();
        foreach (var b in Game1.options.actionButton)
        {
            temp.Add(b.ToSButton());
            Log("Button: " + b);
        }
        Log($"Total {Game1.options.actionButton?.Length ?? 0}");

        ModEntry.ActionButtons = temp;

        LoadClumps();
    }

    /// <summary>
    /// Sets every custom clump to a regular vanilla clump's texture. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void BeforeSaving(object sender, SavingEventArgs e)
    {
        List<ResourceClump> clumps = new();
        
        Utility.ForEachLocation(CheckForCustomClumps, true, true);

        foreach (var resource in clumps)
        {
            //give default values of a stone
            resource.textureName.Set("Maps/springobjects");
            resource.parentSheetIndex.Set(672);
            resource.loadSprite();
        }

        return;

        bool CheckForCustomClumps(GameLocation arg)
        {
            foreach (var resource in arg.resourceClumps)
            {
                //if not custom
                if(resource.modData.ContainsKey(ModKeys.ClumpId) == false)
                    continue;
                    
                clumps.Add(resource);
            }

            return true;
        }
    }

    /// <summary>
    /// Re-sets clump texture to their modded one.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void AfterSaving(object sender, SavedEventArgs e)
    {
        LoadClumps();
    }

    private static void LoadClumps()
    {
        List<ResourceClump> clumps = new();
        
        Utility.ForEachLocation(LoadCustomClumps, true, true);
        Utility.ForEachLocation(CheckNodeDuration, true, true);

        foreach (var resource in clumps)
        {
            //if no custom id (shouldn't happen but JIC)
            if (resource.modData.TryGetValue(ModKeys.ClumpId, out var id) == false)
            {
                Log($"Clump at {resource.Location.NameOrUniqueName} {resource.Tile} doesn't seem to have mod data.");
                continue;
            }
            
            //if not found in data
            if(ModEntry.BigClumps.TryGetValue(id, out var data) == false)
            {
                Log($"Couldn't find mod data for custom clump {id}. Resource will stay as default clump.", LogLevel.Info);
                continue;
            }

            //if texture not found
            if (Game1.content.DoesAssetExist<Texture2D>(data.Texture))
            {
                resource.textureName.Set(data.Texture);
            }
            else
            {
                Log($"Couldn't find texture {data.Texture} for clump at {resource.Location.NameOrUniqueName} {resource.Tile}. Resource will stay as default clump.", LogLevel.Info);
                continue;
            }
            
            resource.parentSheetIndex.Set(data.SpriteIndex);
            resource.loadSprite();

            if (resource.modData.TryGetValue(ModKeys.Days, out var dayString) && int.TryParse(dayString, out var days))
            {
                resource.modData[ModKeys.Days] = $"{days + 1}";
            }
        }

        return;
        
        bool LoadCustomClumps(GameLocation arg)
        {
            string removeAfter = null;
            var howLong = 0;
            var needsRemovalCheck = arg?.GetData()?.CustomFields != null && arg.GetData().CustomFields.TryGetValue(ModKeys.ClumpRemovalDays, out removeAfter);
            
            if (needsRemovalCheck)
                int.TryParse(removeAfter, out howLong);
            
            var removalQueue = new List<ResourceClump>();
            
            foreach (var resource in arg.resourceClumps)
            {
                //if not custom
                if(resource.modData.ContainsKey(ModKeys.ClumpId) == false)
                    continue;

                if (needsRemovalCheck && resource.modData.TryGetValue(ModKeys.Days, out var daysSoFar) &&
                    int.TryParse(daysSoFar, out var days))
                {
                    if (howLong <= days)
                    {
                        removalQueue.Add(resource);
                        continue;
                    }
                }
                
                clumps.Add(resource);
            }

            //remove all that have more days than allowed
            foreach (var resourceClump in removalQueue)
            {
                arg.resourceClumps.Remove(resourceClump);
            }
            
            return true;
        }
    }

    private static bool CheckNodeDuration(GameLocation arg)
    {
        if (arg is null)
            return true;
        
        string removeAfter = null;
        int howLong;
        var needsRemovalCheck = arg.GetData()?.CustomFields != null && arg.GetData().CustomFields.TryGetValue(ModKeys.NodeRemovalDays, out removeAfter);
            
        //if there's property, parse. otherwise ignore location
        if (needsRemovalCheck)
        {
            //if couldn't parse days
            if (int.TryParse(removeAfter, out howLong) == false)
            {
                Log($"Couldn'y parse NodeRemovalDays property for location {arg.DisplayName} ({arg.NameOrUniqueName}). Skipping", LogLevel.Info);
                return true;
            }
        }
        else
        {
            return true;
        }
            
        var removalQueue = new List<Object>();
        
        //for each object, do check
        foreach (var obj in arg.Objects.Values)
        {
            if (obj.modData is null)
                continue;
            
            if (obj.modData.TryGetValue(ModKeys.IsFtm, out var ftm) && bool.Parse(ftm))
                continue;

            if (obj.modData.TryGetValue(ModKeys.Days, out var daysSoFar) == false || int.TryParse(daysSoFar, out var days) == false) 
                continue;
            
            obj.modData[ModKeys.Days] = $"{days + 1}";
            
            if (howLong > days) 
                continue;
            
            removalQueue.Add(obj);
        }

        //remove all that have more days than allowed
        foreach (var obj2 in removalQueue)
        {
            arg.Objects.Remove(obj2.TileLocation);
        }

        return true;
    }
}