using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public class Compensation
{
    private static ISalable _heldItem;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    private static int _totalCount;
    private static List<BaseEnchantment> AllEnchantments { get; set; }= new();
    
    /// <summary>
    /// If the traded item is an enchanted tool, return the prismatic shards used.
    /// </summary>
    /// <param name="itemId">The item to consume.</param>
    /// <param name="count">How many to consume.</param>
    public static void Pre_ConsumeTradeItem(string itemId, int count)
    {
        var qualifiedItemId = ItemRegistry.QualifyItemId(itemId);
#if DEBUG
        Log($"Item id: {qualifiedItemId}");
#endif
        if (!qualifiedItemId.StartsWith("(T)")) 
            return;
        
        //get the tool from player inventory
        var allTools = Game1.player.Items.GetById(qualifiedItemId);
        var num = 0;
            
        foreach (var item in allTools)
        {
            var tool = item as Tool;
            if (tool?.enchantments?.Count <= 0) 
                continue;

            if (tool?.enchantments is null)
                continue;
            
            foreach (var enchantment in tool.enchantments)
            {
#if DEBUG
                Log($"Adding enchantment: {enchantment.GetDisplayName()}, level: {enchantment.Level}");
#endif
                //get active menu's item as tool, add enchantment
                ((Game1.activeClickableMenu as ShopMenu)?.heldItem as Tool)?.enchantments.Add(enchantment);
                num++;
            }
        }

        if (num * count <= 0)
            return;

        _totalCount = num * count;
    }

    private static void ReEnchant()
    {
        var item = (Game1.activeClickableMenu as ShopMenu)?.heldItem;

        if (item is not Tool t)
        {
            if (_totalCount <= 0)
                return;

            var prismaticShard = ItemRegistry.Create("(O)74", _totalCount);
            Game1.player.addItemByMenuIfNecessary(prismaticShard);
            return;
        }

        foreach (var enchantment in AllEnchantments)
        {
            t.enchantments.Add(enchantment);
        }
        AllEnchantments.Clear();
        _totalCount = 0;
    }

    internal static void Post_tryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y, bool __result)
    {
#if DEBUG
        Log($"item: {item?.QualifiedItemId}, held item {held_item?.QualifiedItemId}, stock {stockToBuy}, x {x}, y {y}\n          result {__result} and total count {_totalCount}. Enchantments count {AllEnchantments.Count}");
#endif
        /*
        //if no item bought
        if (__result == false)
            return;
        
        //if it's not a tool, compensate
        if (held_item is not Tool t)
        {
            if (_totalCount <= 0)
                return;

            var prismaticShard = ItemRegistry.Create("(O)74", _totalCount);
            Game1.player.addItemByMenuIfNecessary(prismaticShard);
        }*/
    }
}