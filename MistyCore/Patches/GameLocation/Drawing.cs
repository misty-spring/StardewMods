using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MistyCore.Patches;

public partial class GameLocationPatches
{
    /// <summary>
    /// Draw white water in the sea.
    /// </summary>
    /// <param name="__instance">The game location.</param>
    /// <param name="b">The sprite batch.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>Whether to run original method or not.</returns>
    /// <seealso cref="StardewValley.GameLocation.drawWaterTile(SpriteBatch, int, int)"/>
    internal static bool Pre_drawWaterTile(ref GameLocation __instance, SpriteBatch b, int x, int y)
    {
        if (Game1.IsFading() || Game1.isWarping)
            return true;

        if (__instance.TryGetMapProperty("mistycore.WaterColor", out var colorProperty) == false)
            return true;
        
        if (Utility.StringToColor(colorProperty) is not { } color)
            return true;
        
        //__instance.drawWaterTile(b, x, y, c);
        
        var num = y == __instance.map.Layers[0].LayerHeight - 1 ? 1 : (!__instance.waterTiles[x, y + 1] ? 1 : 0);
        var flag = y == 0 || !__instance.waterTiles[x, y - 1];
        b.Draw(ModEntry.CustomCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (!flag ? (int) __instance.waterPosition : 0))), new Rectangle(__instance.waterAnimationIndex * 64, 2064 + ((x + y) % 2 == 0 ? (__instance.waterTileFlip ? 128 : 0) : (__instance.waterTileFlip ? 0 : 128)) + (flag ? (int) __instance.waterPosition : 0), 64, 64 + (flag ? (int) -(double) __instance.waterPosition : 0)), color, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
        
        if (num == 0)
            return false;
        
        b.Draw(ModEntry.CustomCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int) __instance.waterPosition)), new Rectangle(__instance.waterAnimationIndex * 64, 2064 + ((x + (y + 1)) % 2 == 0 ? (__instance.waterTileFlip ? 128 : 0) : (__instance.waterTileFlip ? 0 : 128)), 64, 64 - (int) (64.0 - __instance.waterPosition) - 1), color, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
        
        return false;
    }

    /// <summary>
    /// Draw lava instead of water.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="b"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <seealso cref="StardewValley.Locations.Caldera"/>
    internal static void Post_drawWaterTile(ref GameLocation __instance, SpriteBatch b, int x, int y)
    {
        if (Game1.IsFading() || Game1.isWarping)
            return;

        if (__instance.TryGetMapProperty("mistycore.DrawLava", out _) == false)
            return;

        //set vars here
        var waterColor = Color.White;
        var mapBaseTilesheet = ModEntry.Help.GameContent.Load<Texture2D>("Maps/Mines/volcano_dungeon");
        var map = __instance.Map;
        var waterTiles = __instance.waterTiles;
        var waterPosition = __instance.waterPosition;
        var waterAnimationIndex = __instance.waterAnimationIndex;
        var waterTileFlip = __instance.waterTileFlip;

        //taken from Caldera
        var num = y == map.Layers[0].LayerHeight - 1 || !waterTiles[x, y + 1];
        var flag = y == 0 || !waterTiles[x, y - 1];
        var num2 = 0;
        var num3 = 320;
        b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!flag) ? waterPosition : 0f))), new Rectangle(num2 + waterAnimationIndex * 16, num3 + (((x + y) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)) + (flag ? ((int)waterPosition / 4) : 0), 16, 16 + (flag ? ((int)(0f - waterPosition) / 4) : 0)), waterColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);

        if (num)
            b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)waterPosition)), new Rectangle(num2 + waterAnimationIndex * 16, num3 + (((x + (y + 1)) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)), 16, 16 - (int)(16f - waterPosition / 4f) - 1), waterColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
    }
}