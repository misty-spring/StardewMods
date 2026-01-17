using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace MistyCore.Patches;

public class TreePatches
{
    private static void Log(string msg, LogLevel lv = ModEntry.Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": prefixing SDV method \"Tree.performTreeFall\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), "performTreeFall"),
            prefix: new HarmonyMethod(typeof(TreePatches), nameof(Pre_performTreeFall))
        );
        
        Log($"Applying Harmony patch \"{nameof(TreePatches)}\": postfixing SDV method \"Tree.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Tree), "performToolAction"),
            postfix: new HarmonyMethod(typeof(TreePatches), nameof(Post_performToolAction))
        );
    }

    private static void Pre_performTreeFall(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        if (ModEntry.Config.TreeFallEvents == false)
            return;
        
        if (__instance.stump.Value)
        {
#if DEBUG
            Log("Tree is already stump. Nothing will be done.");
#endif
            return;
        }
        
        if (ModEntry.TreeFallEvents.TryGetValue(__instance.Location.Name, out var allData) == false)
        {
#if DEBUG
            Log("Couldn't find location data.");
#endif
            return;
        }

        if (allData.TryGetValue(__instance.treeType.Value, out var treeData) == false)
        {
#if DEBUG
            Log("Couldn't find tree data.");
#endif
            return;
        }

        if (Game1.random.NextDouble() > treeData.Chance || GameStateQuery.CheckConditions(treeData.Condition, __instance.Location, t.lastUser) == false)
        {
#if DEBUG
            Log("Either the chance didn't match, or the conditions didn't match.");
#endif
            return;
        }

        foreach (var action in treeData.TriggerActions)
        {
            if (TriggerActionManager.TryRunAction(action, out var error, out var exception) == false)
            {
                Log($"Error while running action: {error}. {exception}");
            }
        }
        
        if (string.IsNullOrWhiteSpace(treeData.PlayEvent))
        {
#if DEBUG
            Log("PlayEvent is null or empty.");
#endif
            return;
        }

        Game1.PlayEvent(treeData.PlayEvent, false, treeData.CheckEventSeen);
    }

    private static void Post_performToolAction(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
    {
        if (ModEntry.Config.TreeChopEvents == false)
            return;
        
        if (__instance.stump.Value || __instance.health.Value <= 0)
        {
            return;
        }
        
        if (ModEntry.TreeChopEvents.TryGetValue(__instance.Location.Name, out var allData) == false)
        {
#if DEBUG
            Log("Couldn't find location data.");
#endif
            return;
        }

        if (allData.TryGetValue(__instance.treeType.Value, out var treeData) == false)
        {
#if DEBUG
            Log("Couldn't find tree data.");
#endif
            return;
        }

        if (Game1.random.NextDouble() > treeData.Chance || GameStateQuery.CheckConditions(treeData.Condition, __instance.Location, t.lastUser) == false || string.IsNullOrWhiteSpace(treeData.Text))
        {
#if DEBUG
            Log("Either the chance didn't match, the conditions didn't match, or the string is null/whitespace.");
#endif
            return;
        }
        
        Game1.drawObjectDialogue(TokenParser.ParseText(treeData.Text));
    }
}