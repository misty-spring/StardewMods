using ItemExtensions.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Shops;

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
    
    /*
    /// <summary>
    /// Checks mod's extra trade data.
    /// </summary>
    /// <param name="shop">The shop to check.</param>
    /// <param name="qualifiedId">Qualified ID of the item.</param>
    /// <param name="extraTrades">The extra items to trade, if any.</param>
    /// <returns>Whether trade requires extra items.</returns>
    bool HasExtraRequirements(string shop, string qualifiedId, out Dictionary<string,int> extraTrades);*/
}

//remove all of this ↓ when copying to your mod
public class Api : IApi
{
    public bool IsResource(string id, out int? health)
    {
        health = null;
        
        if (!ModEntry.Resources.TryGetValue(id, out var resource))
            return false;

        if (resource is null || resource == new ResourceData())
            return false;
        
        health = resource.Health;
        return true;
    }
    
    public bool IsResource(string id, out int? health, out string itemDropped)
    {
        health = null;
        itemDropped = null;
        
        if (!ModEntry.Resources.TryGetValue(id, out var resource))
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

    public bool HasMenuBehavior(string qualifiedItemId) => ModEntry.MenuActions.ContainsKey(qualifiedItemId);
    
    public bool HasBehaviorFor(string item, string target)
    {
        if (!ModEntry.MenuActions.TryGetValue(item, out var value))
            return false;

        var behavior = value.Find(b => b.TargetID == target);
        return behavior != null;
    }

    /*
    public bool HasExtraRequirements(string shop, string qualifiedId, out Dictionary<string, int> extraTrades)
    {
        extraTrades = new Dictionary<string, int>();
        if (!ModEntry.ExtraTrades.TryGetValue(shop, out var shopData))
            return false;
        
        if (!shopData.TryGetValue(qualifiedId, out var itemData))
            return false;

        foreach (var extra in itemData)
        {
            extraTrades.Add(extra.QualifiedItemId, extra.Count);
        }

        return true;
    }*/

    public bool HasExtraRequirements_deprecated(string shop, string shopItemId, out Dictionary<string,int> extraTrades)
    {
        extraTrades = new Dictionary<string, int>();
        
        if (!DataLoader.Shops(Game1.content).TryGetValue(shop, out var shopData))
            return false;

        ShopItemData shopItem = null;
        foreach (var item in shopData.Items)
        {
            if (item.Id != shopItemId)
                continue;

            shopItem = item;
        }

        if (shopItem?.CustomFields is null)
            return false;

        if (!shopItem.CustomFields.TryGetValue(Additions.ModKeys.ExtraTradesKey, out var tradesFromKey))
            return false;

        var parsed = ArgUtility.SplitBySpace(tradesFromKey);
        var skipNext = false;

        for (var i = 0; i < parsed.Length - 1; i++)
        {
            if (skipNext)
            {
                skipNext = false;
                continue;
            }

            int.TryParse(parsed[i + 1], out var count);
            extraTrades.Add(parsed[i], count);
            skipNext = true;
        }

        return true;
    }

    /// <summary>
    /// Checks for stored menu behavior.
    /// </summary>
    /// <param name="item">Qualified ID.</param>
    /// <param name="target">The item we want the behavior of.</param>
    /// <param name="replacesFor"></param>
    /// <param name="conditions"></param>
    /// <param name="trigger"></param>
    /// <returns>Whether there's menu behavior for this item.</returns>
    public bool HasBehaviorFor(string item, string target, out string replacesFor, out string conditions, out string trigger)
    {
        replacesFor = null;
        conditions = null;
        trigger = null;
        
        if (!ModEntry.MenuActions.TryGetValue(item, out var value))
        {
            return false;
        }

        var data = value.Find(m => m.TargetID == target);
        replacesFor = data.ReplaceBy;
        conditions = data.Conditions;
        trigger = data.TriggerActionID;
        
        return true;
    }
    
    public bool HasBehaviorFor(string item, string target, out int qualityChange, out char? modifier)
    {
        qualityChange = -1;
        modifier = null;
        
        if (!ModEntry.MenuActions.TryGetValue(item, out var value))
        {
            return false;
        }

        var data = value.Find(m => m.TargetID == target);
        qualityChange = data.ActualQuality;
        modifier = data.GetQualityModifier();
        
        return true;
    }
}