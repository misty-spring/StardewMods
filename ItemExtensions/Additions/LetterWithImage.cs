using ItemExtensions.Models.Contained;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

// ReSharper disable PossibleLossOfFraction

namespace ItemExtensions.Additions;

public class LetterWithImage : LetterViewerMenu
{
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.Mon.Log(msg, lv);
    private bool HasImage { get; set; }
    private Vector2 ImagePosition { get; set; } = new Vector2(-999, -999);
    private string Text { get; set; }
    
    public LetterWithImage(NoteData note) : base(note.Message)
    {
        if (int.TryParse(note.LetterTexture, out var which))
        {
            letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            whichBG = which;
        }
        else
        {
            letterTexture = Game1.temporaryContent.Load<Texture2D>(note.LetterTexture);
            whichBG = -1;
        }

        var hasText = !string.IsNullOrWhiteSpace(note.Message);
        if (hasText)
        {
            Text = note.Message;
        }
        
        HasImage = !string.IsNullOrWhiteSpace(note.Image);
        if (!HasImage) 
            return;
        
        secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>(note.Image);
        ImagePosition = hasText ? GetPosition(note.ImagePosition, secretNoteImageTexture) : GetCentered(secretNoteImageTexture);
    }

    /// <summary>
    /// Get the position for a given texture.
    /// </summary>
    /// <param name="text">The alignment.</param>
    /// <param name="tex">The texture.</param>
    private static Vector2 GetPosition(string text, Texture2D tex)
    {
        try
        {
            var position = new Vector2();
            var alignment = text ?? "mid";

            //var middle = Game1.viewportCenter.ToVector2();
            var w = Game1.viewport.Width;
            var h = Game1.viewport.Height;
            var middleX = w / 2;
            var middleY = h / 2;

            position.Y = alignment switch
            {
                "top" or "up" => 0,
                "bot" or "bottom" or "down" => h - tex.Height * 4,
                _ => middleY - tex.Height * 2,
            };

            position.X = alignment switch
            {
                "left" => 0,
                "right" => h - tex.Width * 4,
                _ => middleX - tex.Width * 2,
            };

            return position;
        }
        catch (Exception e)
        {
            Log("Error: " + e, LogLevel.Error);
            throw;
        }
    }

    private static Vector2 GetCentered(Texture2D tex)
    {
        var middleX = Game1.viewport.Width / 2;
        var middleY = Game1.viewport.Height / 2;

        var position = new Vector2(middleX - tex.Width * 2,middleY - tex.Height * 2);
        return position;
    }

    public LetterWithImage(string text) : base(text)
    {
    }

    public LetterWithImage(int secretNoteIndex) : base(secretNoteIndex)
    {
    }

    public LetterWithImage(string mail, string mailTitle, bool fromCollection = false) : base(mail, mailTitle, fromCollection)
    {
    }
    
    public override void draw(SpriteBatch b)
    {
        if (!Game1.options.showClearBackgrounds)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        
        //draw note
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(whichBG * 320, 0, 320, 180), Color.White, 0.0f, new Vector2(160f, 90f), 4f * scale, SpriteEffects.None, 0.86f);
        
        if (HasImage)
        {
            b.Draw(
                secretNoteImageTexture, 
                position:ImagePosition, 
                sourceRectangle: new Rectangle(0,0, secretNoteImageTexture.Width, secretNoteImageTexture.Height), 
                color: Color.Black * 0.4f, 
                rotation: 0.0f, 
                origin: Vector2.Zero, 
                scale: 4f, 
                SpriteEffects.None, 
                layerDepth: 0.865f);
                
            b.Draw(
                texture: secretNoteImageTexture, 
                position: ImagePosition, 
                sourceRectangle: new Rectangle(0,0, secretNoteImageTexture.Width, secretNoteImageTexture.Height), 
                color: Color.White, 
                rotation: 0.0f, 
                origin: Vector2.Zero, 
                scale: 4f, 
                effects: SpriteEffects.None, 
                layerDepth: 0.865f);
            
            forwardButton.draw(b);
            backButton.draw(b);
        }
        if (!string.IsNullOrWhiteSpace(Text))
        {
            var textPosition = new Rectangle(xPositionOnScreen + 200, yPositionOnScreen + 100, SpriteText.getWidthOfString(Text), SpriteText.getHeightOfString(Text));
            SpriteText.drawString(
                b: b, 
                s: Text, 
                x: textPosition.X, 
                y: textPosition.Y, 
                width: textPosition.Width, //- 64,
                height: textPosition.Height,
                alpha: 0.75f, 
                layerDepth: 0.865f, 
                color: getTextColor()
                );
        }
        
        if (upperRightCloseButton != null || shouldDrawCloseButton())
            upperRightCloseButton?.draw(b);
        
        if (Game1.options.SnappyMenus && scale < 1.0 || Game1.options.SnappyMenus && !forwardButton.visible && !backButton.visible && !HasQuestOrSpecialOrder && !itemsLeftToGrab())
            return;
        
        drawMouse(b);
    }
}