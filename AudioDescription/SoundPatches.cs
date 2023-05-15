using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace AudioDescription
{
    internal class SoundPatches
    {
        internal static void PostFix_playSound(string cueName)
        {
            #if DEBUG
            ModEntry.Mon.Log("sound: " + cueName);
            ModEntry.Mon.Log("is in allowedcues?: " + ModEntry.AllowedCues?.Contains(cueName));
            #endif
            
            // ReSharper disable once PossibleInvalidOperationException
            if (!(bool)(ModEntry.AllowedCues?.Contains(cueName)))
            {
                return;
            }

            if (ModEntry.LastSound == cueName && ModEntry.Cooldown != 0)
                return;

            ModEntry.LastSound = cueName;
            ModEntry.Cooldown = ModEntry.Config.CoolDown;

            string desc = ModEntry.Help.Translation.Get(cueName);

            if(desc == null)
                return;

            //depending on chosen type: either send as HUDm, or to the sounds box.
            if(ModEntry.Config.Type == ModEntry.NotifType[0])
            {
                Game1.addHUDMessage(new HUDMessage(desc, ModEntry.NexusId));
            }
            else
            {
                ModEntry.SoundMessages.Add(new SoundInfo(desc));
            }
        }

        internal static void PostFix_playSoundPitched(string cueName, int pitch) => PostFix_playSound(cueName);

        internal static void PostFix_makeSound(ref FarmAnimal instance)
        {
            if (ModEntry.Config.NpCs == false)
                return;

            if (!Game1.options.muteAnimalSounds)
            {
                PostFix_playSound(instance.sound.Value);
            }
        }

        internal static bool PrefixHuDdraw(ref HUDMessage instance, SpriteBatch b, int i)
        {
            if (instance.whatType != ModEntry.NexusId)
                return true;
            else
            {
                var titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
                if (instance.noIcon)
                {
                    var overrideX = titleSafeArea.Left + 16;
                    var overrideY = ((Game1.uiViewport.Width < 1400) ? (-64) : 0) + titleSafeArea.Bottom - (i + 1) * 64 * 7 / 4 - 21 - (int)Game1.dialogueFont.MeasureString(instance.message).Y;
                    IClickableMenu.drawHoverText(b, instance.message, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, overrideX, overrideY, instance.transparency);
                    return false;
                }

                // ReSharper disable once PossibleLossOfFraction
                Vector2 vector = new(titleSafeArea.Left + 16, titleSafeArea.Bottom - (i + 1) * 64 * 7 / 4 - 64);
                if (Game1.isOutdoorMapSmallerThanViewport())
                {
                    vector.X = Math.Max(titleSafeArea.Left + 16, -Game1.uiViewport.X + 16);
                }

                if (Game1.uiViewport.Width < 1400)
                {
                    vector.Y -= 48f;
                }

                b.Draw(Game1.mouseCursors, vector, new Rectangle(293, 360, 26, 24), Color.White * instance.transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                var x = Game1.smallFont.MeasureString(instance.message ?? "").X;
                b.Draw(Game1.mouseCursors, new Vector2(vector.X + 104f, vector.Y), new Rectangle(319, 360, 1, 24), Color.White * instance.transparency, 0f, Vector2.Zero, new Vector2(x, 4f), SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(vector.X + 104f + x, vector.Y), new Rectangle(323, 360, 6, 24), Color.White * instance.transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                vector.X += 16f;
                vector.Y += 16f;

                #region customized icon
                b.Draw(
                    ModEntry.MuteIcon, 
                    vector + new Vector2(8f, 8f) * 4f, 
                    new Rectangle(0,0,11,11), 
                    Color.White * instance.transparency, 
                    0f, 
                    new Vector2(6f, 6f), // origin. old: 8f,8f 
                    4f + Math.Max(0f, (instance.timeLeft - 3000f) / 900f),
                    SpriteEffects.None, 
                    1f);
                #endregion

                vector.X += 51f;
                vector.Y += 51f;
                if (instance.number > 1)
                {
                    Utility.drawTinyDigits(instance.number, b, vector, 3f, 1f, Color.White * instance.transparency);
                }

                vector.X += 32f;
                vector.Y -= 33f;
                Utility.drawTextWithShadow(b, instance.message, Game1.smallFont, vector, Game1.textColor * instance.transparency, 1f, 1f, -1, -1, instance.transparency);
                return false;
            }
        }
    }
}