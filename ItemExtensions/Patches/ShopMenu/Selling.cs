using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using ItemExtensions.Models.Internal;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public partial class ShopMenuPatches
{
    private static Dictionary<ISalable, List<ExtraTrade>> ExtraBySalable { get; set; }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        //var instructionsToInsert = new List<CodeInstruction>();

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
        
        if (index <= -1) 
            return codes.AsEnumerable();
        
        /* if (TryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y))
         * {
         *      ...etc
         * }
         */

        var newInstruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(TryToPurchaseItem)));
        foreach (var label in codes[index].labels)
        {
            newInstruction.labels.Add(label);
        }
        
        Log("Inserting method");
        codes[index] = newInstruction;
        
        /* print the IL code
         * courtesy of atravita
         */
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
        
        return codes.AsEnumerable();
    }

    internal static bool TryToPurchaseItem(ShopMenu menu, ISalable item, ISalable held_item, int stockToBuy, int x, int y)
    {
        //if og method returns false
        var tryPurchase = Reflection.GetMethod(menu, "tryToPurchaseItem");
        var result = tryPurchase.Invoke<bool>(item, held_item, stockToBuy, x, y);
#if DEBUG
        Log($"Result: {result}.");
#endif
        if (result == false)
        {
#if DEBUG
            Log($"Can't buy {item.QualifiedItemId} with minimum requirements.");
#endif
            return false;
        }
        
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

        if (stockToBuy > 0 && valid)
        {
            ReduceExtraItems(item, stockToBuy);
            return true;
        }

        if (menu.itemPriceAndStock[item].Price > 0)
            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
        Game1.playSound("cancel");
        return false;
    }
    
    private static void Post_tryToPurchaseItem(ShopMenu __instance, ISalable item, ISalable held_item, int stockToBuy, int x, int y, ref bool __result)
    {
        if (__result == false)
        {
#if DEBUG
            Log("Result is false.");
#endif
            return;
        }
        
        //if no data
        if (ExtraBySalable is not { Count: > 0 })
        {
#if DEBUG
            Log("ExtraBySalable is empty.");
#endif
            return;
        }
        
        //if item not in salable list
        if (!ExtraBySalable.ContainsKey(item))
        {
#if DEBUG
            Log($"ExtraBySalable doesn't have a key for item {item.QualifiedItemId}.");
            return;
#endif
        }
        
        var valid = CanPurchase(item, stockToBuy);

        if (valid)
        {
            ReduceExtraItems(item, stockToBuy);
        }
        else
        {
            if (__instance.itemPriceAndStock[item].Price > 0)
                Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
            Game1.playSound("cancel");
            __result = false;
        }
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
    
    private static bool CanPurchase(ISalable item, int stockToBuy)
    {
        if (Game1.player.Money < item.salePrice() * stockToBuy)
            return false;
        
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