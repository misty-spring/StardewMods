using ItemExtensions.Models.Contained;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace ItemExtensions.Models.Items;

public class ItemData
{
    //misc
    public int MaximumStack { get; set; } = -1;
    public bool HideItem { get; set; } = false;

    //more capability (?)
    public LightData Light { get; set; } = null;
        
    //when x happens
    public OnBehavior OnEquip { get; set; } = null;
    public OnBehavior OnUnequip { get; set; } = null;
    public OnBehavior OnUse { get; set; } = null;
    public OnBehavior OnDrop { get; set; } = null;
    public OnBehavior OnPurchase { get; set; } = null;
    public OnBehavior OnAttached { get; set; } = null;
    public OnBehavior OnDetached { get; set; } = null;

    // customize eating
    public EdibleDialogue FoodDialogue { get; set; } = null;
    public FarmerAnimation Eating { get; set; } = null;
    public FarmerAnimation AfterEating { get; set; } = null;
}