using Microsoft.Xna.Framework;
using MistyCore.Additions.EventCommands;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MistyCore.Events;

public class Display
{
    public static void RenderedHud(object sender, RenderedHudEventArgs e)
    {
        if (!Context.IsWorldReady || !Game1.eventUp)
        {
            return;
        }

        if (Game1.CurrentEvent?.playerControlSequenceID is null || Game1.CurrentEvent.playerControlSequenceID.StartsWith(World.HuntSequenceId) == false)
            return;

        var realId = Game1.CurrentEvent.playerControlSequenceID.Remove(0, 33);
        if (ModEntry.ObjectHunt.TryGetValue(realId, out var data) == false || data.Timer <= 0)
            return;
        
        if (Game1.CurrentEvent.festivalTimer < 0)
            return;
        
        e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(32, 32, 224, 160), Color.Black * 0.5f);
        Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", Game1.CurrentEvent.festivalTimer / 1000), Color.Black, Color.Yellow, new Vector2(64f, 64f), 0.0f, 1f, 1f, false);
        Game1.drawWithBorder(string.Format(ModEntry.Help.Translation.Get("Points").ToString(), Game1.player.festivalScore), Color.Black, Color.Pink, new Vector2(64f, 128f), 0.0f, 1f, 1f, false);
    }
}