using StardewValley;
using StardewValley.Enchantments;

namespace ItemExtensions.Patches;

public class Compensation
{

    private static int _totalCount;
    private static List<BaseEnchantment> AllEnchantments { get; set; }= new();
    
    /// <summary>
    /// If the traded item is an enchanted tool, return the prismatic shards used.
    /// </summary>
    /// <param name="itemId">The item to consume.</param>
    /// <param name="count">How many to consume.</param>
    public static void Pre_ConsumeTradeItem(string itemId, int count)
    {
        _totalCount = 0;
        AllEnchantments.Clear();
        
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
                Log($"Enchantment: {enchantment.GetDisplayName()}, level: {enchantment.Level}");
#endif
                AllEnchantments.Add(enchantment);
                num++;
            }
        }

        if (num * count <= 0)
            return;

        _totalCount = num * count;
        Game1.delayedActions.Add(new DelayedAction(10, ReEnchant));
    }

    private static void ReEnchant()
    {
        var item = Game1.player.ActiveItem;

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

    private void Post_tryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y, bool __result)
    {
        if (__result == false || held_item is not Tool t)
            return;
    }
}