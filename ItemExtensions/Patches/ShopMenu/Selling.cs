using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using ItemExtensions.Models.Internal;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Triggers;

namespace ItemExtensions.Patches;

public partial class ShopMenuPatches
{
    private static Dictionary<ISalable, List<ExtraTrade>> ExtraBySalable { get; set; }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var codes = new List<CodeInstruction>(instructions);

        //find the index of the code instruction we want
        var index = -1;
        for (var i = 2; i < codes.Count - 1; i++)
        {
            if (codes[i-1].opcode != OpCodes.Ldarg_2)
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

    internal static bool TryToPurchaseItem(ShopMenu menu, ISalable item, ISalable heldItem, int stockToBuy, int x, int y)
    {
        if (menu.readOnly)
        {
            return false;
        }
        var stock = menu.itemPriceAndStock[item];
        var isStorageShop = ModEntry.Help.Reflection.GetField<bool>(menu, "_isStorageShop").GetValue();

        if (heldItem == null)
        {
            if (stock.Stock == 0)
            {
                menu.hoveredItem = null;
                return true;
            }
            if (stockToBuy > item.GetSalableInstance().maximumStackSize())
            {
                stockToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
            }
            var price = stock.Price * stockToBuy;
            string extraTradeItem = null;
            var extraTradeItemCount = 5;
            var stacksToBuy = stockToBuy * item.Stack;
            if (stock.TradeItem != null)
            {
                extraTradeItem = stock.TradeItem;
                if (stock.TradeItemCount.HasValue)
                {
                    extraTradeItemCount = stock.TradeItemCount.Value;
                }
                extraTradeItemCount *= stockToBuy;
            }
            if (ShopMenu.getPlayerCurrencyAmount(Game1.player, menu.currency) >= price && (extraTradeItem == null || menu.HasTradeItem(extraTradeItem, extraTradeItemCount)) && IsExtraTradeValid(item, stockToBuy)) //<-- NEW CODE
            {
                menu.heldItem = item.GetSalableInstance();
                menu.heldItem.Stack = stacksToBuy;
                if (!menu.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && !item.IsRecipe)
                {
                    Game1.playSound("smallSelect");
                    menu.heldItem = null;
                    return false;
                }
                if (menu.CanBuyback() && menu.buyBackItems.Contains(item))
                {
                    menu.BuyBuybackItem(item, price, stacksToBuy);
                    BuyBackExtraTrades(item, stacksToBuy);
                }
                ShopMenu.chargePlayer(Game1.player, menu.currency, price);
                if (!string.IsNullOrEmpty(extraTradeItem))
                {
                    menu.ConsumeTradeItem(extraTradeItem, extraTradeItemCount);
                }

                ReduceExtraItems(item, stockToBuy); //<-- NEW LINE
                
                if (!isStorageShop && item.actionWhenPurchased(menu.ShopId))
                {
                    if (item.IsRecipe)
                    {
                        (item as Item)?.LearnRecipe();
                        Game1.playSound("newRecipe");
                    }
                    heldItem = null;
                    menu.heldItem = null;
                }
                else
                {
                    if ((menu.heldItem as Item)?.QualifiedItemId == "(O)858")
                    {
                        Game1.player.team.addQiGemsToTeam.Fire(menu.heldItem.Stack);
                        menu.heldItem = null;
                    }
                    if (Game1.mouseClickPolling > 300)
                    {
                        if (menu.purchaseRepeatSound != null)
                        {
                            Game1.playSound(menu.purchaseRepeatSound);
                        }
                    }
                    else if (menu.purchaseSound != null)
                    {
                        Game1.playSound(menu.purchaseSound);
                    }
                }
                if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
                {
                    menu.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
                    if (stock.ItemToSyncStack != null)
                    {
                        stock.ItemToSyncStack.Stack = stock.Stock;
                    }
                }
                var actionsOnPurchase = stock.ActionsOnPurchase;
                if (actionsOnPurchase != null && actionsOnPurchase.Count > 0)
                {
                    foreach (var action in stock.ActionsOnPurchase)
                    {
                        if (!TriggerActionManager.TryRunAction(action, out var error, out var ex))
                        {
                            Log($"({ex}) Shop {menu.ShopId} ignored invalid action '{action}' on purchase of item '{item.QualifiedItemId}': {error}", LogLevel.Error);
                        }
                    }
                }
                if (menu.onPurchase != null && menu.onPurchase(item, Game1.player, stockToBuy, stock))
                {
                    menu.exitThisMenu();
                }
            }
            else
            {
                if (price > 0)
                {
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                }
                Game1.playSound("cancel");
            }
        }
        else if (heldItem.canStackWith(item))
        {
            stockToBuy = Math.Min(stockToBuy, (heldItem.maximumStackSize() - heldItem.Stack) / item.Stack);
            var stacksToBuy2 = stockToBuy * item.Stack;
            if (stockToBuy > 0)
            {
                var price2 = stock.Price * stockToBuy;
                string extraTradeItem2 = null;
                var extraTradeItemCount2 = 5;
                if (stock.TradeItem != null)
                {
                    extraTradeItem2 = stock.TradeItem;
                    if (stock.TradeItemCount.HasValue)
                    {
                        extraTradeItemCount2 = stock.TradeItemCount.Value;
                    }
                    extraTradeItemCount2 *= stockToBuy;
                }
                var salableInstance = item.GetSalableInstance();
                salableInstance.Stack = stacksToBuy2;
                if (!salableInstance.CanBuyItem(Game1.player))
                {
                    Game1.playSound("cancel");
                    return false;
                }
                if (ShopMenu.getPlayerCurrencyAmount(Game1.player, menu.currency) >= price2 && (extraTradeItem2 == null || menu.HasTradeItem(extraTradeItem2, extraTradeItemCount2)) && IsExtraTradeValid(item, stockToBuy))
                {
                    menu.heldItem.Stack += stacksToBuy2;
                    if (menu.CanBuyback() && menu.buyBackItems.Contains(item))
                    {
                        menu.BuyBuybackItem(item, price2, stacksToBuy2);
                    }
                    ShopMenu.chargePlayer(Game1.player, menu.currency, price2);
                    if (Game1.mouseClickPolling > 300)
                    {
                        if (menu.purchaseRepeatSound != null)
                        {
                            Game1.playSound(menu.purchaseRepeatSound);
                        }
                    }
                    else if (menu.purchaseSound != null)
                    {
                        Game1.playSound(menu.purchaseSound);
                    }
                    if (extraTradeItem2 != null)
                    {
                        menu.ConsumeTradeItem(extraTradeItem2, extraTradeItemCount2);
                    }

                    ReduceExtraItems(item, stockToBuy); //<-- NEW LINE

                    if (!isStorageShop && item.actionWhenPurchased(menu.ShopId))
                    {
                        menu.heldItem = null;
                    }
                    if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
                    {
                        menu.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
                        if (stock.ItemToSyncStack != null)
                        {
                            stock.ItemToSyncStack.Stack = stock.Stock;
                        }
                    }
                    if (menu.onPurchase != null && menu.onPurchase(item, Game1.player, stockToBuy, stock))
                    {
                        menu.exitThisMenu();
                    }
                }
                else
                {
                    if (price2 > 0)
                    {
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    }
                    Game1.playSound("cancel");
                }
            }
        }
        if (stock.Stock <= 0)
        {
            menu.buyBackItems.Remove(item);
            menu.hoveredItem = null;
            return true;
        }
        return false;
    }

    private static bool IsExtraTradeValid(ISalable item, int stockToBuy)
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
        
        var valid = CanPurchase(item, stockToBuy);
        return valid;
    }

    private static void BuyBackExtraTrades(ISalable item, int stacksToBuy)
    {
        Log($"Restoring {stacksToBuy} '{item.DisplayName}' ({item.QualifiedItemId}) extra trades...");
        
        var data = GetData(item);
        
        if (data is null)
            return;

        foreach (var extra in data)
        {
            Log($"Adding {extra.Data.DisplayName} by {extra.Count * stacksToBuy}...");

            Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create(extra.QualifiedItemId, extra.Count * stacksToBuy));
        }
    }
    
    private static void ReduceExtraItems(ISalable item, int stockToBuy)
    {
        Log($"Buying {stockToBuy} {item.DisplayName}...");
        
        var data = GetData(item);
        
        if (data is null)
            return;

        foreach (var extra in data)
        {
            Log($"Reducing {extra.Data.DisplayName} by {extra.Count * stockToBuy}...");

            Compensation.Pre_ConsumeTradeItem(extra.QualifiedItemId, extra.Count * stockToBuy);
            Game1.player.Items.ReduceId(extra.QualifiedItemId, extra.Count * stockToBuy);
        }
    }
    
    private static bool CanPurchase(ISalable item, int stockToBuy)
    {
#if DEBUG
        Log($"item: {item.DisplayName}, stockToBuy {stockToBuy}, in _extraSaleItems {ExtraBySalable.Count}");
#endif

        var data = GetData(item);
        
        if (data is null)
            return false;

        foreach (var extra in data)
        {
#if DEBUG
            Log($"Checking match for {extra.QualifiedItemId}...");
#endif
            if (HasMatch(Game1.player, extra, stockToBuy))
                continue;
            
            return false;
        }

        return true;
    }

    private static List<ExtraTrade> GetData(ISalable item)
    {
        //if extrabysalable has no data
        if (ExtraBySalable is not { Count: > 0 })
        {
#if DEBUG
            Log("No data found in mod Salable list.");
#endif
        }
        
        
        //if data wasn't in salable
        if (ExtraBySalable.TryGetValue(item, out var data) == false)
        {
#if DEBUG
            Log("Id not found in mod Salables.");
#endif
        }

        return data;
    }
    
    private static bool HasMatch(Farmer farmer, ExtraTrade c, int bought = 1)
    {
        var all = farmer.Items.GetById(c.QualifiedItemId);
        var total = 0;

        foreach (var item in all)
        {
#if DEBUG
            Log($"item {item.QualifiedItemId}, stack {item.Stack}");
#endif
            if (item.QualifiedItemId != c.QualifiedItemId)
                continue;

            total += item.Stack;
        }

        var withStock = c.Count * bought;
        
#if DEBUG
        Log($"total {total}, withStock {withStock}, total >= withStock {total >= withStock}");
#endif
        return total >= withStock;
    }
}