using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class TractorModPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        var attachments = new[] { "Axe", "Custom", "Hoe", "MeleeBlunt", "MeleeDagger", "MeleeSword", "Scythe", "Slingshot", "WateringCan" };

        foreach (var toolType in attachments)
        {
            var tractorToolMethod = AccessTools.Method($"Pathoschild.Stardew.TractorMod.Framework.Attachments.{toolType}Attachment:Apply",
                new[] { typeof(Vector2),typeof(Object),typeof(TerrainFeature),typeof(Farmer),typeof(Tool),typeof(Item),typeof(GameLocation) });

            if (tractorToolMethod is null) //if the method isn't found, return
            {
                Log($"Method not found. ({toolType}Attachment:Apply)", LogLevel.Warn);
                continue;
            }        
            Log($"Applying Harmony patch \"{nameof(TractorModPatches)}\": postfixing mod method \"Pathoschild.Stardew.TractorMod.Framework.Attachments.{toolType}Attachment.Apply\".");
        
            harmony.Patch(
                original: tractorToolMethod,
                postfix: new HarmonyMethod(typeof(TractorModPatches), nameof(Post_OnActivated))
            );
        }
    }

    private static void Post_OnActivated(Vector2 tile, Object? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
    {
        try
        {
            if (tool is null)
                return;
            
            tileObj?.performToolAction(tool);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
}