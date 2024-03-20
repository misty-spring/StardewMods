using HarmonyLib;
using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Triggers;

namespace ItemExtensions.Patches;

public class ItemPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": postfixing SDV method \"Item.addToStack()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Item), nameof(Item.addToStack)),
            postfix: new HarmonyMethod(typeof(ItemPatches), nameof(Post_addToStack))
        );
        
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": postfixing SDV method \"Item.actionWhenPurchased\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Item), nameof(Item.actionWhenPurchased)),
            postfix: new HarmonyMethod(typeof(ItemPatches), nameof(Post_actionWhenPurchased))
        );
        
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": postfixing SDV method \"Item.onEquip\".");
        harmony.Patch(
          original: AccessTools.Method(typeof(Item), nameof(Item.onEquip)),
          postfix: new HarmonyMethod(typeof(ItemPatches), nameof(Post_onEquip))
        );
        
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": postfixing SDV method \"Item.onUnequip\".");
        harmony.Patch(
          original: AccessTools.Method(typeof(Item), nameof(Item.onUnequip)),
          postfix: new HarmonyMethod(typeof(ItemPatches), nameof(Post_onUnequip))
        );
        
        /*
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": postfixing SDV method \"IItemDataDefinition.CreateItem\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(IItemDataDefinition), nameof(IItemDataDefinition.CreateItem)),
            postfix: new HarmonyMethod(typeof(ItemPatches), nameof(Post_CreateItem))
        );*/
    }

    private static void Post_CreateItem(ParsedItemData data, ref Item __result)
    {
        if (__result is not StardewValley.Object o)
            return;
        ObjectPatches.Post_new(ref o, __result.QualifiedItemId, __result.Stack);
    }
    
    #region triggers
    public static void Post_addToStack(Item otherStack)
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_AddedToStack");
    }
    
    public static void Post_actionWhenPurchased(Item __instance, string shopId)
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_OnPurchased");
      
        #if DEBUG
        Log($"Called OnPurchased, id {__instance.QualifiedItemId}");
        #endif
        
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.OnPurchase == null)
            return;
      
        ActionButton.CheckBehavior(mainData.OnPurchase);
    }
    
    /// <summary>Handle the item being equipped by the player (i.e. added to an equipment slot, or selected as the active tool).</summary>
    /// <param name="who">The player who equipped the item.</param>
    public static void Post_onEquip(Item __instance, Farmer who)
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_OnEquip");
      
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.OnEquip == null)
            return;
      
        ActionButton.CheckBehavior(mainData.OnEquip);
    }

    /// <summary>Handle the item being unequipped by the player (i.e. removed from an equipment slot, or deselected as the active tool).</summary>
    /// <param name="who">The player who unequipped the item.</param>
    public static void Post_onUnequip(Item __instance, Farmer who)
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_OnUnequip");
      
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.OnUnequip == null)
            return;
      
        ActionButton.CheckBehavior(mainData.OnUnequip);
    }
    #endregion
}