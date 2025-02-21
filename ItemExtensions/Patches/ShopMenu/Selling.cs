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
        var instructionsToInsert = new List<CodeInstruction>();

        var index = -1;
        for (var i = 2; i < codes.Count - 1; i++)
        {
            if (codes[i-1].opcode != OpCodes.Ldloc_0)
                continue;
            
            if(codes[i].opcode != OpCodes.Ldfld)
                continue;
            
            if(codes[i + 1].opcode != OpCodes.Brfalse_S)
                continue;

            index = i + 1;
            break;
        }
        
        var redirectTo = codes.Find(ci => codes.IndexOf(ci) == index);
        
#if DEBUG
        Log($"index: {index}", LogLevel.Info);
#endif
        
        if (index <= -1) 
            return codes.AsEnumerable();
        
        //add label for brtrue
        var brtrueLabel = il.DefineLabel();
        redirectTo.labels ??= new List<Label>();
        redirectTo.labels.Add(brtrueLabel);
        
        /* if (stock.TradeItem != null)
           {
               ...
           }
           if (CanExtraTrade(item, held_item, stockToBuy) == false)
               return;
         */

        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_2));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_3));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopMenuPatches), nameof(CanExtraTrade))));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue, brtrueLabel));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ret));
        
        Log("Inserting method");
        codes.InsertRange(index, instructionsToInsert);
        
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

    internal static bool CanExtraTrade(ISalable item, ISalable heldItem, int stockToBuy)
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

        if (stockToBuy <= 0 || !valid) 
            return false;
        
        ReduceExtraItems(item, stockToBuy);
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