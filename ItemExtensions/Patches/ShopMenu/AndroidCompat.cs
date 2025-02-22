using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public partial class ShopMenuPatches
{
    private static IEnumerable<CodeInstruction> Transpiler_Android(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var codes = new List<CodeInstruction>(instructions);

        //find the index of the code instruction we want
        var index = -1;
        for (var i = 2; i < codes.Count - 1; i++)
        {
            if (codes[i-1].opcode != OpCodes.Ldc_I4_0)
                continue;
            
            if(codes[i].opcode != OpCodes.Call)
                continue;
            
            if(codes[i + 1].opcode != OpCodes.Brfalse_S)
                continue;

            index = i;
            break;
        }
#if DEBUG
        Log($"index: {index}", LogLevel.Info);
#endif
        
        //if not found return original
        if (index <= -1) 
            return codes.AsEnumerable();
        
        /* if (TryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y))
         * {
         *      ...etc
         * }
         */

        //create call instruction with our method
        var newInstruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(TryToPurchaseItem)));
        foreach (var label in codes[index].labels)
        {
            newInstruction.labels.Add(label);
        }
        
        Log("Inserting method");
        codes[index] = newInstruction;
        
        /* print the IL code
         * courtesy of atravita

        StringBuilder sb = new();
        sb.Append("ILHelper for: ShopMenu.receiveLeftClick");
        for (int i = 0; i < codes.Count; i++)
        {
            sb.AppendLine().Append(codes[i]);
            if (index == i)
            {
                sb.Append("       <---- single transpiler");
            }
        }
        Log(sb.ToString(), LogLevel.Info);
        */
        return codes.AsEnumerable();
    }

    /// <summary>
    /// Tries to purchase an item including extra item trades.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="item"></param>
    /// <param name="held_item"></param>
    /// <param name="stockToBuy"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="__result">Result this method gives</param>
    /// <returns>Whether to run original method.</returns>
    internal static bool Pre_tryToPurchaseItem(ShopMenu __instance, ISalable item, ISalable held_item, int stockToBuy, int x, int y, ref bool __result)
    {
        if (ExtraBySalable is not { Count: > 0 })
        {
#if DEBUG
            Log("ExtraBySalable is empty.");
#endif
            return true;
        }
        
        //if item not in salable list
        if (!ExtraBySalable.ContainsKey(item))
        {
#if DEBUG
            Log($"ExtraBySalable doesn't have a key for item {item.QualifiedItemId}.");
#endif
            return true;
        }
        
        __result = TryToPurchaseItem(__instance, item, held_item, stockToBuy, x, y);
        return false;
    }

    private static void Post_drawMobileToolTip(IClickableMenu __instance, SpriteBatch b, int x, int y, int width, int height, int paragraphGap, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, string extraItemToShowIndexStr = null, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1, int currency = 0, bool inStockAndBuyable = true, bool drawSmall = false)
    {
        // in android's ShopMenu.cs: 
        // IClickableMenu.drawMobileToolTip(b, priceX + 16, priceY + 16, priceWidth - 32, priceHeight - 32, 34, descItem, nameItem, currentItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), null, priceItem, currency, getPlayerCurrencyAmount(Game1.player, currency) >= itemPriceAndStock[forSale[currentlySelectedItem]].Price, drawSmall: true);
        if(Game1.activeClickableMenu is not ShopMenu) 
            return;
        
        if(string.IsNullOrWhiteSpace(extraItemToShowIndexStr) || hoveredItem == null) 
            return;
        
        //if no salable data
        if(ExtraBySalable is not { Count: > 0 })
            return;
        
        if(!InDictionary(hoveredItem, out var data))
            return;
        
        DrawExtraItems_Android(data, b, new Vector2(x + 100, y + 420));
    }

    private static void DrawExtraItems_Android(List<ExtraTrade> data, SpriteBatch spriteBatch, Vector2 position)
    {
        var fixedPosition = position; 
        fixedPosition.X = position.X;
        
        foreach (var item in data)
        {
            fixedPosition.X += 100;
            spriteBatch.Draw(item.Data.GetTexture(), fixedPosition, item.Data.GetSourceRect(), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Game1.dialogueFont, $"x{item.Count}", new Vector2(fixedPosition.X + 50, fixedPosition.Y), Color.Black);
        }
    }
}