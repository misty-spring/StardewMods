using System.Collections.Generic;
using StardewValley;

namespace FarmVisitors.Datamodels;

internal class DupeNPC
{
    internal static void SetVariables(NPC who)
    {
        //general data
        who.CurrentDialogue = new Stack<Dialogue>();
        who.ignoreScheduleToday = true;
        who.temporaryController = null;
        who.currentLocation = Utility.getHomeOfFarmer(Game1.player);
        who.Position = Utility.getHomeOfFarmer(Game1.player).getEntryLocation().ToVector2();
        who.Schedule?.Clear();
        who.Dialogue?.Clear();
        who.goingToDoEndOfRouteAnimation.Value = false;
        who.Breather = who.Breather;
    }

    internal static NPC Duplicate(NPC who)
    {
        var sprite = new AnimatedSprite(who.getTextureName(), 0, who.Sprite.SpriteWidth, who.Sprite.SpriteHeight);
        var position = Utility.getHomeOfFarmer(Game1.player).getEntryLocation().ToVector2();
        var facing = who.FacingDirection;
        var name = who.Name;

        var result = new NPC(sprite, position, facing, name)
        {
            displayName = who.displayName,
            Gender = who.Gender,
            Age = who.Age,
            Portrait = who.Portrait,
            Manners = who.Manners,
            Optimism = who.Optimism,
            SocialAnxiety = who.SocialAnxiety,
            currentLocation = Utility.getHomeOfFarmer(Game1.player),
            CurrentDialogue = new Stack<Dialogue>(),
            ignoreScheduleToday = true,
            temporaryController = null,
            Position = Utility.getHomeOfFarmer(Game1.player).getEntryLocation().ToVector2(),
            Breather = who.Breather
           };

        result.CurrentDialogue?.Clear();
        result.Schedule?.Clear();
        result.Dialogue?.Clear();
        result.ClearSchedule();
        result.Halt();
        result.goingToDoEndOfRouteAnimation.Value = false;
        result.Sprite = sprite;
        result.Sprite.ClearAnimation();
        
        return result;
    }
}