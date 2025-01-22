using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models.Items;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.Tools;

namespace ItemExtensions.Patches;

internal class FishingRodPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level = LogLevel.Trace;
#endif

    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(FishingRodPatches)}\": postfixing game method \"FishingRod.openTreasureMenuEndFunction\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.openTreasureMenuEndFunction)),
            postfix: new HarmonyMethod(typeof(FishingRodPatches), nameof(Post_openTreasureMenuEndFunction))
        );
    }

    internal static void Post_openTreasureMenuEndFunction(FishingRod __instance, int remainingFish)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu menu)
        {
#if DEBUG
            Log("Not item grab menu.", LogLevel.Warn);
#endif
            return;
        }

        if (menu.source != 3)
            return;
        
        var context = new ItemQueryContext(__instance.lastUser.currentLocation, __instance.lastUser, Game1.random, "ItemExtensions' Post_openTreasureMenuEndFunction");
        
        foreach (var (entry, data) in ModEntry.Treasure)
        {
#if DEBUG
            Log($"Checking entry {entry}...");
#endif
            if (RodMatch(data, __instance) == false)
                continue;
            
            if (Sorter.GetItem(data, context, out var item) == false)
                continue;

            menu.ItemsToGrabMenu.actualInventory.Add(item);
            
            Log($"Added treasure reward from entry {entry} ({item.QualifiedItemId})");
        }
    }

    /// <summary>
    /// Checks for rod conditions.
    /// </summary>
    /// <param name="data">The spawn data.</param>
    /// <param name="rod">The rod to check</param>
    /// <returns>Whether all conditions match.</returns>
    private static bool RodMatch(TreasureData data, FishingRod rod)
    {
        if (rod is null)
            return false;
        
        try
        {
            //rods
            if (data.Rod.Any() && data.Rod.Contains(rod.ItemId) == false)
                return false;
            //bait
            if (rod.GetBait() != null)
            {
                if (data.Bait.Any() && data.Bait.Contains(rod.GetBait().ItemId) == false)
                    return false;
            }
            //tackle
            if (rod.GetTackle() != null && rod.GetTackle().Any() && data.Tackle.Any())
            {
                //if all tackle must be in list
                if (data.RequireAllTackle)
                {
                    if (rod.GetTackle().TrueForAll(i => data.Tackle.Contains(i.ItemId)) == false)
                        return false;
                }
                else if (rod.GetTackle().Any(i => data.Tackle.Contains(i.ItemId)) == false)
                    return false;
            }
            //attachment limits
            if (data.MinAttachments > 0)
            {
                if (rod.AttachmentSlotsCount < data.MinAttachments)
                    return false;
            }
            if (data.MaxAttachments > -1)
            {
                if (rod.AttachmentSlotsCount > data.MinAttachments)
                    return false;
            }
            //bobber
            if (data.Bobber >= 0 && data.Bobber != rod.getBobberStyle(rod.getLastFarmerToUse()))
                return false;
        }
        catch (Exception e)
        {
            Log($"Error: {e}.\nCheck will be treated as false.", LogLevel.Warn);
            return false;
        }

        return true;
    }
}