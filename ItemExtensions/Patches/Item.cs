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
        CodeMatcher cMatcher = new CodeMatcher(instructions);

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
                AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.getMenuString)))
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
                AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.getMenuString)))
        );

        cMatcher.AddLabels(secondLabel);

        return cMatcher.InstructionEnumeration();
    }
    
    /// <summary>Allows customization of the unable to eat/drink message.</summary>
    /// <param name="instructions">IL code instructions passed to the transpiler.</param>
    static IEnumerable<CodeInstruction> Transpiler_Farmer_eatObject(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher cMatcher = new CodeMatcher(instructions);

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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.getFailureString)))
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranspilerSupplementary), nameof(TranspilerSupplementary.getFailureString)))
            );

        return cMatcher.InstructionEnumeration();
    }
}

public class TranspilerSupplementary
{
    /// <summary>Internally used by the Game1.pressActionButton transpiler.</summary>
    /// <param name="stringFallback">A string referencing a game content string.</param>
    /// <param name="obj">The object being eaten.</param>
    public static string getMenuString(string stringFallback, Object obj)
    {
        var modified = Game1.content.Load<Dictionary<string, Dictionary<string, string>>>("placeholder");
        
        if (modified.ContainsKey(obj.ItemId) && modified[obj.ItemId].ContainsKey("ConsumeText"))
        {
            return modified[obj.ItemId]["ConsumeText"].Replace("{0}", obj.DisplayName);
        }

        return Game1.content.LoadString(stringFallback).Replace("{0}", obj.DisplayName);
    }

    /// <summary>Internally used by the Farmer.eatObject transpiler.</summary>
    /// <param name="stringFallback">A string referencing a game content string.</param>
    /// <param name="obj">The object unable to be eaten.</param>
    public static string getFailureString(string stringFallback, Object obj)
    {
        var modified = Game1.content.Load<Dictionary<string, Dictionary<string, string>>>("placeholder");

        if (modified.ContainsKey(obj.ItemId) && modified[obj.ItemId].ContainsKey("CannotConsumeText"))
        {
            return modified[obj.ItemId]["CannotConsumeText"].Replace("{0}", obj.DisplayName);
        }

        return Game1.content.LoadString(stringFallback).Replace("{0}", obj.DisplayName);
    }
}