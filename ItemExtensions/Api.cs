using ItemExtensions.Models;
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
    /// <returns>Whether the object has ore data.</returns>
    bool IsResource(string id, out int? health);
    
    /// <summary>
    /// Checks for resource data in the mod.
    /// </summary>
    /// <param name="id">Qualified item ID</param>
    /// <param name="health">MinutesUntilReady value</param>
    /// <param name="itemDropped">Item dropped by ore</param>
    /// <returns>Whether the object has ore data.</returns>
    bool IsResource(string id, out int? health, out string itemDropped);
    
    /// <summary>
    /// Checks for light data.
    /// </summary>
    /// <param name="id">Qualified item ID.</param>
    /// <param name="light">Light color</param>
    /// <returns>Whether this item has light data.</returns>
    bool HasLight(string id, out Color? light);
    
    /// <summary>
    /// Checks mod's menu behaviors.
    /// </summary>
    /// <param name="qualifiedItemId">Qualified item ID.</param>
    /// <returns>Whether this item has any menu behavior</returns>
    bool HasMenuBehavior(string qualifiedItemId);

    /// <summary>
    /// Checks mod's menu behaviors.
    /// For checking general behavior, see <see cref="HasMenuBehavior"/>.
    /// </summary>
    /// <param name="qualifiedItemId">Qualified item ID.</param>
    /// <param name="target">Item to search behavior for. (Qualified item ID)</param>
    /// <returns>Whether this item has menu behavior for target.</returns>
    bool HasBehaviorFor(string qualifiedItemId, string target);
    
    /// <summary>
    /// Checks mod's extra trade data.
    /// </summary>
    /// <param name="shop">The shop to check.</param>
    /// <param name="qualifiedItemId">Qualified ID of the item.</param>
    /// <param name="extraTrades">The extra items to trade, if any.</param>
    /// <returns>Whether trade requires extra items.</returns>
    bool HasExtraRequirements(string shop, string qualifiedItemId, out Dictionary<string,int> extraTrades);
}

//remove all of this ↓ when copying to your mod
public class Api : IApi
{
    public bool IsResource(string id, out int? health)
    {
        health = null;
        
        if (!ModEntry.Data.TryGetValue(id, out var data))
            return false;

        if (data.Resource is null || data.Resource == new ResourceData())
            return false;
        
        health = data.Resource.Health;
        return true;
    }
    
    public bool IsResource(string id, out int? health, out string itemDropped)
    {
        health = null;
        itemDropped = null;
        
        if (!ModEntry.Data.TryGetValue(id, out var data))
            return false;

        if (data.Resource is null || data.Resource == new ResourceData())
            return false;
        
        health = data.Resource.Health;
        itemDropped = data.Resource.ItemDropped;
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

    public bool HasMenuBehavior(string qualifiedItemId) => ModEntry.ItemActions.ContainsKey(qualifiedItemId);
    
    public bool HasBehaviorFor(string item, string target)
    {
        if (!ModEntry.ItemActions.TryGetValue(item, out var value))
            return false;

        var behavior = value.Find(b => b.TargetID == target);
        return behavior != null;
    }
    
    public bool HasExtraRequirements(string shop, string qualifiedItemId, out Dictionary<string,int> extraTrades)
    {
        extraTrades = new Dictionary<string, int>();
        
        if (!ModEntry.ExtraTrades.TryGetValue(shop, out var shopData))
            return false;

        if(!shopData.TryGetValue(qualifiedItemId, out var trades))
            return false;

        foreach (var extra in trades)
        {
            extraTrades.Add(extra.QualifiedItemId, extra.Count);
        }

        return true;
    }

    #region requires custom models
    /// <summary>
    /// Checks for resource data in the mod.
    /// </summary>
    /// <param name="id">Qualified item ID</param>
    /// <param name="resource">ResourceData, if found.</param>
    /// <returns>Whether it's a resource object.</returns>
    public bool IsResource(string id, out ResourceData resource)
    {
        if (!ModEntry.Data.TryGetValue(id, out var data))
        {
            resource = null;
            return false;
        }

        resource = data.Resource;
        
        //check that it isn't null AND not default values
        return data.Resource != null && data.Resource != new ResourceData();
    }

    /// <summary>
    /// Checks for item data in the mod.
    /// </summary>
    /// <param name="id">Qualified item ID</param>
    /// <param name="item">Itemdata, including: light, resource, onBehavior(s), etc.</param>
    /// <returns>Whether the mod has data for this item.</returns>
    public bool HasItemData(string id, out ItemData item) => ModEntry.Data.TryGetValue(id, out item);

    /// <summary>
    /// Checks for stored menu behavior.
    /// </summary>
    /// <param name="item">Qualified ID.</param>
    /// <param name="target">The item we want the behavior of.</param>
    /// <param name="behavior">The obtained behavior, if any.</param>
    /// <returns>Whether there's menu behavior for this item.</returns>
    public bool HasBehaviorFor(string item, string target, out MenuBehavior behavior)
    {
        if (!ModEntry.ItemActions.TryGetValue(item, out var value))
        {
            behavior = null;
            return false;
        }

        behavior = value.Find(b => b.TargetID == target);
        return behavior != null;
    }
    #endregion
}