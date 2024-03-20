using System.Text;
using HarmonyLib;
using ItemExtensions.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public partial class ShopMenuPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    private static IReflectionHelper Reflection => ModEntry.Help.Reflection;

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": prefixing SDV method \"ShopMenu.Initialize\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "Initialize"),
            postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Post_Initialize))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": prefixing SDV method \"ShopMenu.Initialize\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "AddForSale"),
            postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Post_AddForSale))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": postfixing SDV method \"ShopMenu.cleanupBeforeExit\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "cleanupBeforeExit"),
            postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Post_cleanupBeforeExit))
        );
        
       var types = new[]
       {
           typeof(SpriteBatch),
           typeof(StringBuilder),
           typeof(SpriteFont),
           typeof(int),
           typeof(int),
           typeof(int),
           typeof(string),
           typeof(int),
           typeof(string[]),
           typeof(Item),
           typeof(int),
           typeof(string),
           typeof(int),
           typeof(int),
           typeof(int),
           typeof(float),
           typeof(CraftingRecipe),
           typeof(IList<Item>),
           typeof(Texture2D),
           typeof(Rectangle?),
           typeof(Color?),
           typeof(Color?),
           typeof(float),
           typeof(int),
           typeof(int)
       };
      
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": postfixing SDV method \"IClickableMenu.drawHoverText\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), types),
            postfix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Post_drawHoverText))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": prefixing SDV method \"ShopMenu.receiveLeftClick\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveLeftClick)),
            prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Pre_receiveLeftClick))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopMenuPatches)}\": prefixing SDV method \"ShopMenu.receiveLeftClick\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveRightClick)),
            prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(Pre_receiveLeftClick))
        );
    }
    
    internal static void Post_Initialize(ShopMenu __instance, int currency, Func<ISalable, Farmer, int, bool> onPurchase, Func<ISalable, bool> onSell, bool playOpenSound)
    {
        ExtraBySalable = new Dictionary<ISalable, List<ExtraTrade>>();
        ByQualifiedId = new Dictionary<string, List<ExtraTrade>>();
    }

    /// <summary>
    /// Reset sale items.
    /// </summary>
    internal static void Post_cleanupBeforeExit()
    {
        //_extraSaleItems = new Dictionary<string, List<ExtraTrade>>();
        ExtraBySalable.Clear();
        ByQualifiedId.Clear();
    }
    
    /// <summary>
    /// Adds salable items.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="item"></param>
    /// <param name="stock"></param>
    private static void Post_AddForSale(ShopMenu __instance,ISalable item, ItemStockInformation? stock = null)
    {
        var match = FindMatch(__instance, item, stock);
        if (match is null)
        {
            if (ModEntry.Shops.TryGetValue(__instance.ShopId, out var allItemIds) == false)
                return;
            
            if(allItemIds.TryGetValue(item.QualifiedItemId, out var data) == false)
                return;

            ByQualifiedId.Add(item.QualifiedItemId, data);
            return;
        }
        
        if (ExtraTrade.TryParse(match, out var extras) == false)
            return;

        ExtraBySalable.Add(item, extras);
    }

    private static string FindMatch(ShopMenu menu, ISalable item, ItemStockInformation? stock = null)
    {
        foreach (var dataItem in menu.ShopData.Items)
        {
            //don't check those without ID
            if (string.IsNullOrWhiteSpace(dataItem.TradeItemId))
                continue;
            
            //if id doesnt match
            if (dataItem.ItemId != item.QualifiedItemId)
            {
                //if its not assumed object
                if($"(O){dataItem.ItemId}" != item.QualifiedItemId)
                    continue;
            }
            
            //also checks stock data if possible
            if (stock is not null)
            {
                if(dataItem.TradeItemAmount != stock.Value.TradeItemCount)
                    continue;
                
                /*if(dataItem.AvailableStockLimit != stock.Value.LimitedStockMode)
                    continue;*/
            }
            
            if (item.appliesProfitMargins().Equals(dataItem.ApplyProfitMargins) == false)
                continue;

            if (dataItem.IsRecipe != item.IsRecipe)
                continue;

            if (dataItem.MaxStack != item.maximumStackSize())
                continue;

            if (dataItem.CustomFields.TryGetValue(Additions.ModKeys.ExtraTradesKey, out var trades) == false)
                continue;

            return trades;
        }

        return null;
    }

    private static bool InDictionary(Item hoveredItem, out List<ExtraTrade> list)
    {
        list = null;
        
        //prioritize bySalable check
        foreach (var pair in ExtraBySalable)
        {
            if (pair.Key.QualifiedItemId != hoveredItem.QualifiedItemId)
                continue;

            if (pair.Key.IsRecipe != hoveredItem.IsRecipe)
                continue;
            
            if(pair.Key.Quality != hoveredItem.Quality)
                continue;

            if (pair.Key.IsInfiniteStock() != hoveredItem.IsInfiniteStock())
                continue;
            
            if(pair.Key.Stack != hoveredItem.Stack)
                continue;

            list = pair.Value;
            return true;
        }

        //fallback to qualifiedId
        foreach (var pair2 in ByQualifiedId)
        {
            if(pair2.Key != hoveredItem.QualifiedItemId)
                continue;

            list = pair2.Value;
            return true;
        }

        return false;
    }
}