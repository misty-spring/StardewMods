using HarmonyLib;
using ItemExtensions.Additions;
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
        Log($"Applying Harmony patch \"{nameof(FishingRodPatches)}\": postfixing mod method \"FishingRod.openTreasureMenuEndFunction\".");

        harmony.Patch(
            original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.openTreasureMenuEndFunction)),
            postfix: new HarmonyMethod(typeof(FishingRodPatches), nameof(Post_openTreasureMenuEndFunction))
        );
    }

    public static void Post_openTreasureMenuEndFunction(FishingRod __instance, int remainingFish)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu rewards || rewards.source != ItemGrabMenu.source_fishingChest)
            return;

        var context = new ItemQueryContext(__instance.lastUser.currentLocation, __instance.lastUser, Game1.random);

        foreach ((var entry, var data) in ModEntry.Treasure)
        {
#if DEBUG
            Log("Checking entry {entry}...");
#endif
            if (Sorter.GetItem(data, context, out var item) == false)
                continue;

            rewards.ItemsToGrabMenu.tryToAddItem(item);
            Log($"Added treasure reward from entry {entry} ({item.QualifiedItemId})");
        }
    }
}