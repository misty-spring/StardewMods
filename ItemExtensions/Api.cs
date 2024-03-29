using ItemExtensions.Additions.Clumps;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ItemExtensions;

public interface IApi
{
    /// <summary>
    /// Checks for resource data in the mod.
    /// </summary>
    /// <param name="id">Qualified item ID</param>
    /// <param name="health">MinutesUntilReady value</param>
    /// <param name="itemDropped">Item dropped by ore</param>
    /// <returns>Whether the object has ore data.</returns>
    bool IsResource(string id, out int? health, out string itemDropped);
    
    /// <summary>
    /// Checks mod's menu behaviors. If a target isn't provided, it'll search whether any exist.
    /// </summary>
    /// <param name="qualifiedItemId">Qualified item ID.</param>
    /// <param name="target">Item to search behavior for. (Qualified item ID)</param>
    /// <returns>Whether this item has menu behavior for target.</returns>
    bool HasBehavior(string qualifiedItemId, string target);

    bool IsClump(string qualifiedItemId);
    
    bool TrySpawnClump(string itemId, Vector2 position, string locationName, out string error, bool avoidOverlap = false);
    
    bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string error, bool avoidOverlap = false);

    List<string> GetCustomSeeds(string itemId, bool includeSource, bool parseConditions = true);
}

//remove all of this ↓ when copying to your mod
public class Api : IApi
{
    public bool IsResource(string id, out int? health, out string itemDropped)
    {
        health = null;
        itemDropped = null;
        
        if (!ModEntry.Ores.TryGetValue(id, out var resource))
            return false;

        if (resource is null || resource == new ResourceData())
            return false;
        
        health = resource.Health;
        itemDropped = resource.ItemDropped;
        return true;
    }

    public bool HasLight(string id, out Color? light)
    {
        light = null;
        
        if (!ModEntry.Data.TryGetValue(id, out var data))
            return false;

        if (data.Light is null || data.Light == new LightData())
            return false;

        var l = data.Light;
        
        //prioritize hex- if null, use RGB
        light = !string.IsNullOrWhiteSpace(l.Hex) ? Utility.StringToColor(l.Hex) : new Color(l.R, l.G, l.B);
        light *= l.Transparency;
        
        return true;
    }

    public bool HasBehavior(string qualifiedItemId, string target = null)
    {
        if(string.IsNullOrWhiteSpace(target))
            return ModEntry.MenuActions.ContainsKey(qualifiedItemId);
        
        if (!ModEntry.MenuActions.TryGetValue(qualifiedItemId, out var value))
            return false;

        var behavior = value.Find(b => b.TargetId == target);
        return behavior != null;
    }

    public bool IsClump(string qualifiedItemId) => ModEntry.BigClumps.ContainsKey(qualifiedItemId);

    public bool TrySpawnClump(string itemId, Vector2 position, string locationName, out string error, bool avoidOverlap = false) => TrySpawnClump(itemId, position, Utility.fuzzyLocationSearch(locationName), out error, avoidOverlap);
    
    public bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string error, bool avoidOverlap = true)
    {
        error = null;
        
        if(ModEntry.BigClumps.TryGetValue(itemId, out var data) == false)
        {
            error = "Couldn't find the given ID.";
            return false;
        }

        var clump = ExtensionClump.Create(itemId, data, position);

        try
        {
            if(avoidOverlap)
            {
                if (location.IsTileOccupiedBy(position))
                {
                    var width = location.map.DisplayWidth / 64;
                    var height = location.map.DisplayHeight / 64;
                    
                    for (var i = 0; i < 30; i++)
                    {
                        var newPosition = new Vector2(
                            Game1.random.Next(1, width),
                            Game1.random.Next(1, height));
                        
                        if (location.IsTileOccupiedBy(newPosition) || location.IsNoSpawnTile(newPosition) || !location.CanItemBePlacedHere(newPosition))
                            continue;

                        clump.Tile = newPosition;
                    }
                }
            }
            
            location.resourceClumps.Add(clump);
        }
        catch (Exception ex)
        {
            error = $"{ex}";
            return false;
        }

        return true;
    }

    public List<string> GetCustomSeeds(string itemId, bool includeSource, bool parseConditions = true)
    {
        //if no seed data
        if (ModEntry.Seeds.TryGetValue(itemId, out var seeds) == false)
            return null;

        var result = new List<string>();

        foreach (var mixedSeed in seeds)
        {
            if (string.IsNullOrWhiteSpace(mixedSeed.Condition)) 
                continue; 
            
            if (GameStateQuery.CheckConditions(mixedSeed.Condition, Game1.player.currentLocation, Game1.player))
                result.Add(mixedSeed.ItemId);
        }

        return result;
    }
}