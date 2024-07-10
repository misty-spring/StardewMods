using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Triggers;
using Object = StardewValley.Object;

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

        if (ModEntry.Config.OnBehavior == false)
            return;

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
        
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": transpiler running on SDV method \"Game1.pressActionButton\"");
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), nameof(Game1.pressActionButton)),
            transpiler: new HarmonyMethod(typeof(ItemPatches), nameof(Transpiler_Game1_pressActionButton))
        );
        
        Log($"Applying Harmony patch \"{nameof(ItemPatches)}\": transpiler running on SDV method \"Farmer.eatObject\"");
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
            transpiler: new HarmonyMethod(typeof(ItemPatches), nameof(Transpiler_Farmer_eatObject))
        );
    }

    private static void Post_CreateItem(ParsedItemData data, ref Item __result)
    {
        try
        {
            if (__result is not Object o)
                return;
            ObjectPatches.Post_new(ref o, o.TileLocation, o.ItemId, o.IsRecipe);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
    
    public static void Post_addToStack(Item otherStack)
    {
        try
        {
            TriggerActionManager.Raise($"{ModEntry.Id}_AddedToStack");
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
    
    public static void Post_actionWhenPurchased(Item __instance, string shopId)
    {
        try
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
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    /// <summary>Handle the item being equipped by the player (i.e. added to an equipment slot, or selected as the active tool).</summary>
    /// <param name="__instance">Item equipped.</param>
    /// <param name="who">The player who equipped the item.</param>
    public static void Post_onEquip(Item __instance, Farmer who)
    {
        try
        {
            TriggerActionManager.Raise($"{ModEntry.Id}_OnEquip");

            if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
                return;

            if (mainData.OnEquip == null)
                return;

            ActionButton.CheckBehavior(mainData.OnEquip);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    /// <summary>Handle the item being unequipped by the player (i.e. removed from an equipment slot, or deselected as the active tool).</summary>
    /// <param name="__instance">Item unequipped.</param>
    /// <param name="who">The player who unequipped the item.</param>
    public static void Post_onUnequip(Item __instance, Farmer who)
    {
        try
        {
            TriggerActionManager.Raise($"{ModEntry.Id}_OnUnequip");

            if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
                return;

            if (mainData.OnUnequip == null)
                return;

            ActionButton.CheckBehavior(mainData.OnUnequip);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
    
    /// <summary>Allows customization of the eat/drink confirmation message.</summary>
    /// <param name="instructions">IL code instructions passed to the transpiler.</param>
    public static IEnumerable<CodeInstruction> Transpiler_Game1_pressActionButton(IEnumerable<CodeInstruction> instructions)
    {
        var cMatcher = new CodeMatcher(instructions);

        cMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.content))),
                new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:Game1.cs.3160")
            )
            .ThrowIfNotMatch("Couldn't find patch location for pressActionButton.");

        var firstLabel = cMatcher.Labels;

        cMatcher.RemoveInstructions(6);

        cMatcher.Insert(
            new CodeInstruction(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:Game1.cs.3160"),
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeInstruction(OpCodes.Callvirt,
                AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject))),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.GetMenuString)))
        );

        cMatcher.AddLabels(firstLabel);

        cMatcher.Advance(5);

        var secondLabel = cMatcher.Labels;
        
        cMatcher.RemoveInstructions(6);

        cMatcher.Insert(
            new CodeInstruction(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:Game1.cs.3159"),
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeInstruction(OpCodes.Callvirt,
                AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject))),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.GetMenuString)))
        );

        cMatcher.AddLabels(secondLabel);
        
        /*own addition
        var thirdLabel = cMatcher.Labels;
        cMatcher.Advance(4);
#if DEBUG
        Log($"Current: {cMatcher.Instruction}, {cMatcher.Opcode}");
#endif
        cMatcher.RemoveInstructions(2);
        cMatcher.Insert(
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeInstruction(OpCodes.Callvirt,
                AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.currentLocation))),
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeInstruction(OpCodes.Callvirt,
                AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject))),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.GetYesNo)))
        );
        cMatcher.AddLabels(thirdLabel);
        */

        return cMatcher.InstructionEnumeration();
    }
    
    /// <summary>Allows customization of the unable to eat/drink message.</summary>
    /// <param name="instructions">IL code instructions passed to the transpiler.</param>
    private static IEnumerable<CodeInstruction> Transpiler_Farmer_eatObject(IEnumerable<CodeInstruction> instructions)
    {
        var cMatcher = new CodeMatcher(instructions);

        cMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.content))),
                new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:Game1.cs.2898")
            )
            .ThrowIfNotMatch("Couldn't find starting position for eatObject patch #1.")
            .RemoveInstruction()
            .Advance(1)
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.GetFailureString)))
            );
        
        cMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.content))),
                new CodeMatch(OpCodes.Ldstr, "Strings\\StringsFromCSFiles:Game1.cs.2899")
            )
            .ThrowIfNotMatch("Couldn't find starting position for eatObject patch #2.")
            .RemoveInstruction()
            .Advance(1)
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.GetFailureString)))
            );

        return cMatcher.InstructionEnumeration();
    }
}

internal class TranspilerSupplementary
{
    /// <summary>Internally used by the Game1.pressActionButton transpiler.</summary>
    /// <param name="stringFallback">A string referencing a game content string.</param>
    /// <param name="obj">The object being eaten.</param>
    internal static string GetMenuString(string stringFallback, Object obj)
    {
        var fallback = Game1.content.LoadString(stringFallback).Replace("{0}", obj.DisplayName);
        
        if (ModEntry.Data.TryGetValue(obj.QualifiedItemId, out var data) == false)
            return fallback;
        
        if (data.EdibleData is null || string.IsNullOrWhiteSpace(data.EdibleData.ConsumeQuestion))
            return fallback;
        
        return data.EdibleData.ConsumeQuestion.Replace("{0}", obj.DisplayName);
    }

    /// <summary>Internally used by the Farmer.eatObject transpiler.</summary>
    /// <param name="stringFallback">A string referencing a game content string.</param>
    /// <param name="obj">The object unable to be eaten.</param>
    internal static string GetFailureString(string stringFallback, Object obj)
    {
        var fallback = Game1.content.LoadString(stringFallback).Replace("{0}", obj.DisplayName);
        
        if (ModEntry.Data.TryGetValue(obj.QualifiedItemId, out var data) == false)
            return fallback;
        
        if (data.EdibleData is null || string.IsNullOrWhiteSpace(data.EdibleData.CannotConsume))
            return fallback;
        
        return data.EdibleData.CannotConsume.Replace("{0}", obj.DisplayName);
    }

    /*
    internal static Response[] GetYesNo(GameLocation location, Object obj)
    {
        var fallback = location.createYesNoResponses();
        var custom = location.createYesNoResponses();
        
        if (ModEntry.Data.TryGetValue(obj.QualifiedItemId, out var data) == false)
            return fallback;
        
        if (data.EdibleData is null || (string.IsNullOrWhiteSpace(data.EdibleData.Yes) && string.IsNullOrWhiteSpace(data.EdibleData.No)))
            return fallback;

        if(string.IsNullOrWhiteSpace(data.EdibleData.Yes) == false)
            custom[0].responseText = data.EdibleData.Yes;
        
        if(string.IsNullOrWhiteSpace(data.EdibleData.No) == false)
            custom[1].responseText = data.EdibleData.No;

        return custom;
    }*/
}