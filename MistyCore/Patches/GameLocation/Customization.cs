using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MistyCore.Patches;

public partial class GameLocationPatches
{
    internal static void Post_makeHoeDirt(ref GameLocation __instance, Vector2 tileLocation, bool ignoreChecks = false)
    {
        if (ModEntry.HoeDirt.TryGetValue(__instance.Name, out var tex) == false)
            return;
        
        if(!__instance.terrainFeatures.TryGetValue(tileLocation, out var terrainFeature))
            return;

        if(terrainFeature is not HoeDirt dirt)
            return;

        //grab right texture
        var texture2D = Game1.content.Load<Texture2D>(tex);

        var normalTexture = ModEntry.Help.Reflection.GetField<Texture2D>(dirt, "texture");
        normalTexture.SetValue(texture2D);
    }
    
    /// <summary>
    /// Draws the night background in the underwater west zone.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="b"></param>
    /// <seealso cref="StardewValley.Locations.IslandLocation.DrawParallaxHorizon"/>
    private static void Post_drawBackground(GameLocation __instance, SpriteBatch b)
    {
        if (ModEntry.Backgrounds.TryGetValue(__instance.Name, out var data) == false)
            return;
        
        var location = Game1.player.currentLocation;
        var horizontal_parallax = data.HorizontalParallax;
        
        var texture = Game1.content.Load<Texture2D>(data.TexturePath);
        var num1 = 4f;

        if (texture == null || texture.IsDisposed)
        {
            Log("The texture either wasn't found or was disposed of by the game. (Texture: )" + texture, LogLevel.Warn);
            texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cloudy_Ocean_BG");
        }
        
        var num2 = texture.Width * num1 - location.map.DisplayWidth; 
        var t = 0.0f; 
        var num3 = -640; 
        var y = (int) (Game1.viewport.Y * 0.20000000298023224 + num3); 
        if (horizontal_parallax) 
        {
            if (location.map.DisplayWidth - Game1.viewport.Width < 0)
                t = 0.5f;
            else if (location.map.DisplayWidth - Game1.viewport.Width > 0)
                t = Game1.viewport.X / (float) (location.map.DisplayWidth - Game1.viewport.Width);
        }
        else
            t = 0.5f;
        if (Game1.game1.takingMapScreenshot)
        {
            y = num3;
            t = 0.5f;
        }
        var num4 = 0.25f;
        var num5 = Utility.Lerp(0.5f + num4, 0.5f - num4, t);
        var x = (int)(-(double)num2 * num5); //this is what decides the speed at which the background moves.
        var globalPosition = new Rectangle(x / data.Speed, y, (int) (texture.Width * (double) num1), (int) (texture.Height * (double) num1));
        var rectangle = new Rectangle(0, 0, texture.Width, texture.Height); //x divided by 4 so it moves slower
        var num7 = 0;
        if (globalPosition.X < num7)
        {
            var num8 = num7 - globalPosition.X;
            globalPosition.X += num8 / 2;
            globalPosition.Width -= num8 / 2;
            rectangle.X += (int) (num8 / (double) num1);
            rectangle.Width -= (int) (num8 / (double) num1);
        }
        var displayWidth = location.map.DisplayWidth;
        if (globalPosition.X + globalPosition.Width > displayWidth)
        {
            var num9 = globalPosition.X + globalPosition.Width - displayWidth;
            globalPosition.Width -= num9 / 2;
            rectangle.Width -= (int) ((num9 / (double) num1) / 2);
        }
        if (rectangle.Width <= 0 || globalPosition.Width <= 0)
            return;
        b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, globalPosition), rectangle, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
    }
    
}