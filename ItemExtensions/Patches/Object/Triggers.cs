using ItemExtensions.Events;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Triggers;
using Object = StardewValley.Object;
using static ItemExtensions.Additions.ModKeys;

namespace ItemExtensions.Patches;

public partial class ObjectPatches
{
    public static void Post_actionWhenBeingHeld(Farmer who)
    {
        if (ModEntry.Holding)
            return;
      
        TriggerActionManager.Raise($"{ModEntry.Id}_OnBeingHeld");
      
        ModEntry.Holding = true;
    }

    public static void Post_actionWhenStopBeingHeld(Farmer who)
    {
        ModEntry.Holding = false;
        TriggerActionManager.Raise($"{ModEntry.Id}_OnStopHolding");
    }
    
    public static void Post_performRemoveAction()
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_OnItemRemoved");
    }
    
    public static void Post_dropItem(Object __instance, GameLocation location, Vector2 origin, Vector2 destination)
    {
        TriggerActionManager.Raise($"{ModEntry.Id}_OnItemDropped");
      
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.OnDrop == null)
            return;
      
        ActionButton.CheckBehavior(mainData.OnDrop);
    }
    
    public static void Pre_maximumStackSize(Object __instance, ref int __result)
    {
        if(__instance.modData.TryGetValue(StackModData, out var stack))
            __result = int.Parse(stack);
        
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var data))
            return;

        if (data.MaximumStack == 0)
            return;

        __result = data.MaximumStack;
    }
    
    public static void Pre_IsHeldOverHead(Object __instance, ref bool __result)
    {
        if(__instance.modData.TryGetValue(ItemHeadModData, out var boolean))
            __result = bool.Parse(boolean);
        
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var data))
            return;

        __result = data.HideItem;
    }
}