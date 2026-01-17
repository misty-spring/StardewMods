using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace MistyCore.Patches;

public partial class GameLocationPatches
{
    private static void Log(string msg, LogLevel lv = ModEntry.Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        if (ModEntry.Config.CustomHoeDirt)
        {
            Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": postfixing SDV method \"GameLocation.makeHoeDirt\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.makeHoeDirt)),
                postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_makeHoeDirt))
            );
        }

        if (ModEntry.Config.CustomBackgrounds)
        {
            Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": prefixing SDV method \"GameLocation.drawBackground\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawBackground)),
                postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_drawBackground))
            );
        }
        
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": postfixing SDV method \"GameLocation.getFish\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
            postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_getFish))
        );
        
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": prefixing SDV method \"GameLocation.drawWaterTile\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawWaterTile),
                new[] { typeof(SpriteBatch), typeof(int), typeof(int) }),
            prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Pre_drawWaterTile))
        );
        
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": postfixing SDV method \"GameLocation.drawWaterTile\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.drawWaterTile),
                new[] { typeof(SpriteBatch), typeof(int), typeof(int) }),
            postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_drawWaterTile))
        );
    }
}