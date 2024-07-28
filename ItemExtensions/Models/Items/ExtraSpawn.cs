using StardewValley;
using StardewValley.GameData;
using StardewValley.Internal;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace ItemExtensions.Models.Items;

/// <summary>
/// Extra spawns for resources.
/// See <see cref="Game1"/>, or <see cref="StardewValley.Internal.ItemQueryResolver"/>
/// </summary>
public class ExtraSpawn : ISpawnItemData
{
    public bool AvoidRepeat { get; set; } = true;
    public double Chance { get; set; } = 1;
    public string Condition { get; set; } = "TRUE";
    public string ItemId { get; set; } = null;
    public List<string> RandomItemId { get; set; }
    public int? MaxItems { get; set; }
    public QuantityModifier.QuantityModifierMode QualityModifierMode { get; set; }
    public string PerItemCondition { get; set; } = null;
    public int MinStack { get; set; } = 1;
    public int MaxStack { get; set; } = 1;

    public int Quality { get; set; } = 0;

    public string ObjectInternalName { get; set; }
    public string ObjectDisplayName { get; set; }
    public int ToolUpgradeLevel { get; set; }
    public bool IsRecipe { get; set; }
    public List<QuantityModifier> StackModifiers { get; set; }
    public QuantityModifier.QuantityModifierMode StackModifierMode { get; set; }
    public List<QuantityModifier> QualityModifiers { get; set; }
    
    public ItemQuerySearchMode Filter { get; set; } = ItemQuerySearchMode.All;
    public Dictionary<string, string> ModData { get; set; }
    public List<string> AvoidItemIds { get; set; } = new();
}