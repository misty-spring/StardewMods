using HarmonyLib;
using ItemExtensions.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public class ShopPatches
{
    internal static bool InMenu { get; set; }
    internal static Dictionary<string, List<ExtraItems>> ExtraSaleItems { get; set; } = new();
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": prefixing SDV method \"ShopBuilder.GetShopStock\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock), new []{typeof(string)}),
            prefix: new HarmonyMethod(typeof(ShopPatches), nameof(Pre_GetShopStock))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": postfixing SDV method \"ShopMenu.AddForSale\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.AddForSale)),
            postfix: new HarmonyMethod(typeof(ShopPatches), nameof(Post_AddForSale))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": postfixing SDV method \"ShopMenu.cleanupBeforeExit\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "cleanupBeforeExit"),
            postfix: new HarmonyMethod(typeof(ShopPatches), nameof(Post_cleanupBeforeExit))
        );
        
        /*Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": postfixing SDV method \"ShopMenu.draw\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "getHoveredItemExtraItemAmount"),
            postfix: new HarmonyMethod(typeof(ShopPatches), nameof(Post_getHoveredItemExtraItemAmount))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": postfixing SDV method \"ShopMenu.performHoverAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.performHoverAction)),
            postfix: new HarmonyMethod(typeof(ShopPatches), nameof(Post_performHoverAction))
        );*/
        
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": prefixing SDV method \"ShopMenu.tryToPurchaseItem\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
            prefix: new HarmonyMethod(typeof(ShopPatches), nameof(Pre_tryToPurchaseItem))
        );
        
        Log($"Applying Harmony patch \"{nameof(ShopPatches)}\": postfixing SDV method \"ShopMenu.tryToPurchaseItem\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
            postfix: new HarmonyMethod(typeof(ShopPatches), nameof(Post_tryToPurchaseItem))
        );
    }
    
    #region reset values
    internal static void Pre_GetShopStock(string shopId)
    {
        ExtraSaleItems = new Dictionary<string, List<ExtraItems>>();
        InMenu = true;
    }

    /// <summary>
    /// Reset sale items.
    /// </summary>
    internal static void Post_cleanupBeforeExit()
    {
        ExtraSaleItems = new Dictionary<string, List<ExtraItems>>();
        InMenu = false;
    }

    #endregion
    
    #region extra conditions
    private static bool HasMatch(Farmer farmer, ExtraItems c, out Item match)
    {
        foreach (var item in farmer.Items)
        {
            if(item.QualifiedItemId != c.QualifiedItemId)
                continue;
            
            /*if(item.Quality > c.ActualQuality)
                continue;*/
            
            if(item.Stack > c.Count)
                continue;

            match = item;
            return true;
        }

        match = null;
        return false;
    }
    
    internal static void Pre_tryToPurchaseItem(ShopMenu __instance, ISalable item, ISalable held_item, int stockToBuy, int x, int y,
        ref bool __result)
    {
        if(!__result)
            return;
        
        if(ExtraSaleItems is not { Count: > 0 })
            return;
        
        if(!ExtraSaleItems.TryGetValue(item.QualifiedItemId, out var data))
            return;

        foreach (var extra in data)
        {
            if (!HasMatch(Game1.player, extra, out var match))
                continue;

            if(match.Stack < extra.Count * stockToBuy)
                __result = false;
        }
    }
    #endregion

    #region using values
    internal static void Post_tryToPurchaseItem(ShopMenu __instance, ISalable item, ISalable held_item, int stockToBuy, int x, int y,
        ref bool __result)
    {
        if(!__result)
            return;
        
        if(ExtraSaleItems is not { Count: > 0 })
            return;
        
        if(!ExtraSaleItems.TryGetValue(item.QualifiedItemId, out var data))
            return;

        foreach (var extra in data)
        {
            if (!HasMatch(Game1.player, extra, out var match))
                continue;

            match.ConsumeStack(extra.Count * stockToBuy);
        }
    }
    internal static void Post_AddForSale(ref ShopMenu __instance, ISalable item, ItemStockInformation? stock = null)
    {
        if (!ModEntry.ExtraTrades.TryGetValue(__instance.ShopId, out var data))
            return;
        
        if(!data.TryGetValue(item.QualifiedItemId, out var itemData))
            return;

        ExtraSaleItems.TryAdd(item.QualifiedItemId, itemData);
    }

    internal static void Post_getHoveredItemExtraItemAmount(ShopMenu __instance)
    {
        if(__instance.hoverText == null)
            return;
        
        if(ExtraSaleItems is not { Count: > 0 })
            return;
        
        if(!ExtraSaleItems.TryGetValue(__instance.hoveredItem.QualifiedItemId, out var data))
            return;

        foreach (var item in data)
        {
            //
        }
    }
    #endregion

    /*
    internal static void Post_draw(ShopMenu __instance, SpriteBatch b)
    {
        if(ExtraSaleItems is not { Count: > 0 })
            return;
        
        if(HoverRecipe is null || _x <= -1 || _y <= -1)
            return;
        
        //IClickableMenu.drawTextureBox(b, _x - 16, _y - 16, width,64, Color.White);
        //Utility.drawTextWithShadow(b, _islandBtn.label, Game1.smallFont, whereFrom, Game1.textColor);

        var v = __instance.VisualTheme;
        IClickableMenu.drawToolTip(b, " ", __instance.boldTitleText, __instance.hoveredItem as Item, __instance.heldItem != null, currencySymbol: __instance.currency, extraItemToShowIndex: null, extraItemToShowAmount: 0, craftingIngredients: HoverRecipe, moneyAmountToShowAtBottom: __instance.hoverPrice > 0 ? __instance.hoverPrice : -1);
        /*IClickableMenu.drawTextureBox(v.ItemRowBackgroundTexture, v.ItemRowBackgroundSourceRect, __instance.forSaleButtons[index].bounds.X, __instance.forSaleButtons[index].bounds.Y, __instance.forSaleButtons[index].bounds.Width, __instance.forSaleButtons[index].bounds.Height, !__instance.forSaleButtons[index].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) || v.ItemRowBackgroundHoverColor, 4f, false);
        
        ///
        var position = new Vector2(_x, _y);
        b.Draw(_islandBtn.texture,position,_islandBtn.sourceRect,Color.White,0,Vector2.Zero,4f,SpriteEffects.None,(float) (0.8600000143051147 + (double) _islandBtn.bounds.Y / 20000.0));
        
        //text
        var whereFrom = new Vector2(_islandBtn.bounds.X + _islandBtn.bounds.Width + 8, _islandBtn.bounds.Y - 12);
        Utility.drawTextWithShadow(b, _islandBtn.label, Game1.smallFont, whereFrom, Game1.textColor);*//*
    }

    /// <summary>
    /// Fixes to show extra required items, if any.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <see cref="GameLocation.ActivateKitchen"/>
    internal static void Post_performHoverAction(ShopMenu __instance, int x, int y)
    {
        if(ExtraSaleItems is not { Count: > 0 })
            return;

        for (var index = 0; index < __instance.forSaleButtons.Count; ++index)
        {
            if (__instance.currentItemIndex + index >= __instance.forSale.Count ||
                !__instance.forSaleButtons[index].containsPoint(x, y))
            {
                HoverRecipe = null;
                continue;
            }
            
            var key = __instance.forSale[__instance.currentItemIndex + index];
            if (!ExtraSaleItems.TryGetValue(key.QualifiedItemId, out var data))
            {
                HoverRecipe = null;
                return;
            }

            if (__instance.canPurchaseCheck != null &&
                !__instance.canPurchaseCheck(__instance.currentItemIndex + index))
            {
                HoverRecipe = null;
                return;
            }
            
            //__instance.hoverText = key.getDescription();
            //__instance.boldTitleText = key.DisplayName;
            
            HoverRecipe = new CraftingRecipe("Iron Fence", false)
            {
                description = __instance.hoverText,
                DisplayName = key.DisplayName,
                numberProducedPerCraft = 0,
                itemToProduce = null,
                recipeList = GetAsDictionary(data),
                timesCrafted = 0
            };

            _x = x;
            _y = y;

            break;
            /*__instance.hoverPrice =
                __instance.itemPriceAndStock == null ||
                !__instance.itemPriceAndStock.TryGetValue(key, out var stockInformation)
                    ? key.salePrice()
                    : stockInformation.Price;

            __instance.hoveredItem = key;
            __instance.forSaleButtons[index].scale =
                Math.Min(__instance.forSaleButtons[index].scale + 0.03f, 1.1f);*//*
        }
    }

    private static Dictionary<string, int> GetAsDictionary(List<ExtraItems> data)
    {
        var result = new Dictionary<string, int>();
        foreach (var item in data)
        {
            result.Add(item.Data.QualifiedItemId, item.Count);
        }

        return result;
    }
    
    private static string GetIdForSale(ShopData instanceShopData, ISalable stock)
    {
        foreach (var data in instanceShopData.Items)
        {
            if(data.ItemId == stock.QualifiedItemId)
            if(data.ApplyProfitMargins != stock.appliesProfitMargins())
                continue;

            var flag = false;
            if (stock.QualifiedItemId.StartsWith("(O)"))
            {
                var rawId = stock.QualifiedItemId.Remove(0, 3);
                flag = data.ItemId != rawId;
            }
            
            if(flag && data.ItemId != stock.QualifiedItemId)
                continue;
            
            if(data.IsRecipe != stock.IsRecipe)
                continue;
            
        }
        throw new ArgumentOutOfRangeException();
    }*/
}