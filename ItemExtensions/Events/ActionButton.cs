using ItemExtensions.Additions;
using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace ItemExtensions.Events;

public static class ActionButton
{
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.Mon.Log(msg, lv);
    public static void Pressed(object sender, ButtonPressedEventArgs e)
    {
        if(!Context.IsWorldReady)
            return;
        
        if(!ModEntry.ActionButtons.Contains(e.Button))
            return;

        if (Game1.player.ActiveObject == null)
            return;

        if (!ModEntry.Data.TryGetValue(Game1.player.ActiveObject.ItemId, out var data))
            return;

        if (data.OnUse == null)
            return;
        
        CheckBehavior(data.OnUse, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
    }

    public static void CheckBehavior(OnBehavior behavior, int xTile, int yTile, GameLocation location)
    {
        if (!string.IsNullOrWhiteSpace(behavior.Conditions) && !GameStateQuery.CheckConditions(behavior.Conditions))
        {
            Log("Conditions for item don't match.");
            return;
        }

        var hasConfirm = !string.IsNullOrWhiteSpace(behavior.Confirm);
        var hasReject = !string.IsNullOrWhiteSpace(behavior.Reject);
        
        if (!string.IsNullOrWhiteSpace(behavior.Message) && (hasConfirm || hasReject))
        {
            var defaultResponse = Game1.currentLocation.createYesNoResponses();
            
            var responses = new[]
            {
                hasConfirm ? new Response("Yes", TokenParser.ParseText(behavior.Confirm)) : defaultResponse[0],
                hasReject ? new Response("No", TokenParser.ParseText(behavior.Reject)) : defaultResponse[1]
            };

            void AfterDialogueBehavior(Farmer who, string whichanswer)
            {
                if(whichanswer == "Yes")
                    RunBehavior(behavior, xTile, yTile, location, false);
            }

            Game1.currentLocation.createQuestionDialogue(TokenParser.ParseText(behavior.Message), responses, AfterDialogueBehavior);
        }
        else
        {
            RunBehavior(behavior, xTile, yTile, location, true);
        }
    }

    private static void RunBehavior(OnBehavior behavior, int xTile, int yTile, GameLocation location, bool directAction)
    {
        if (behavior.ReduceBy > 0)
            Game1.player.ActiveObject.ConsumeStack(behavior.ReduceBy);

        if (!string.IsNullOrWhiteSpace(behavior.ChangeMoney))
        {
            Game1.player.Money = IWorldChangeData.ChangeValues(behavior.ChangeMoney, Game1.player.Money, Game1.player.Money);
        }
        
        IWorldChangeData.Solve(behavior);
        
        if(directAction && !string.IsNullOrWhiteSpace(behavior.Message))
        {
            Game1.addHUDMessage(new HUDMessage(TokenParser.ParseText(behavior.Message),2));
        }

        if (behavior.ShowNote != null && GameStateQuery.CheckConditions(behavior.ShowNote.Condition))
        {
            var note = behavior.ShowNote;
            if (!string.IsNullOrWhiteSpace(note.MailId))
            {
                if (DataLoader.Mail(Game1.content).TryGetValue(note.MailId, out var rawMail))
                {
                    var mail = TokenParser.ParseText(rawMail);
                    Game1.activeClickableMenu = new LetterViewerMenu(mail);
                }
            }
            else if (!string.IsNullOrWhiteSpace(note.Message) || !string.IsNullOrWhiteSpace(note.Image))
            {
                var menu = new LetterWithImage(note);
                Game1.activeClickableMenu = menu;
            }

            if (note.AddFlags != null)
            {
                foreach (var flag in note.AddFlags)
                {
                    Game1.player.mailReceived.Add(flag);
                }
            }
        }
        
        var monsters = behavior.SpawnMonsters;
        
        if (monsters is null) 
            return;

        var tileLocation = new Vector2(xTile, yTile);
        
        foreach (var monster in monsters)
        {
            var tile = tileLocation + monster.Distance;
            if (location.IsTileOccupiedBy(tile))
            {
                tile = ClosestOpenTile(location,tile);
                Log($"Changing tile position to {tile}...");
            }

            var mon = Sorter.GetMonster(monster, tile * 64, Game1.random.Next(0, 3));
                    
            //calculates drops
            var drops = new List<string>();
            if(monster.ExcludeOriginalDrops == false)
                drops.AddRange(mon.objectsToDrop);
        
            var context = new ItemQueryContext(location, Game1.player, Game1.random, "ItemExtensions' RunBehavior method");
            //for each one do chance & parse query
            foreach (var drop in monster.ExtraDrops)
            {
                if(drop.Chance < Game1.random.NextDouble())
                    continue;

                if (Sorter.GetItem(drop, context, out var item) == false)
                    continue;
                        
                drops.Add(item.QualifiedItemId);
            }

            mon.objectsToDrop.Set(drops);
                    
            location.characters.Add(mon);
        }
    }
    
    internal static Vector2 ClosestOpenTile(GameLocation location, Vector2 tile)
    {
        for (var i = 1; i < 30; i++)
        {
            var toLeft = new Vector2(tile.X - i, tile.Y);
            if (!location.IsTileOccupiedBy(toLeft))
            {
                return toLeft;
            }
            
            var toRight = new Vector2(tile.X + i, tile.Y);
            if (!location.IsTileOccupiedBy(toRight))
            {
                return toRight;
            }
            
            var toUp = new Vector2(tile.X, tile.Y - i);
            if (!location.IsTileOccupiedBy(toUp))
            {
                return toUp;
            }
            
            var toDown = new Vector2(tile.X, tile.Y + i);
            if (!location.IsTileOccupiedBy(toDown))
            {
                return toDown;
            }

            var upperLeft= new Vector2(tile.X - i, tile.Y - 1);
            if (!location.IsTileOccupiedBy(upperLeft))
            {
                return upperLeft;
            }
            
            var lowerLeft= new Vector2(tile.X - i, tile.Y + 1);
            if (!location.IsTileOccupiedBy(lowerLeft))
            {
                return lowerLeft;
            }
            
            var upperRight= new Vector2(tile.X + i, tile.Y - 1);
            if (!location.IsTileOccupiedBy(upperRight))
            {
                return upperRight;
            }
            
            var lowerRight= new Vector2(tile.X + i, tile.Y + 1);
            if (!location.IsTileOccupiedBy(lowerRight))
            {
                return lowerRight;
            }
        }

        return tile;
    }
}