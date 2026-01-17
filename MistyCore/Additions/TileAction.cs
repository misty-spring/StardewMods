using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace MistyCore.Additions;

public static class TileAction
{
    private static void Log(string str) => ModEntry.Mon.Log(str);
    
    public static bool AddItem(GameLocation arg1, string[] arg2, Farmer arg3, Point arg4)
    {
        if (arg2 is null || arg2.Length < 2 || ModEntry.AddItemTileAction.TryGetValue(arg2[1], out var data) == false)
            return false;

        if (GameStateQuery.CheckConditions(data.Condition) == false)
        {
            if (string.IsNullOrWhiteSpace(data.MessageToShowIfConditionsFail))
            {
                return false;
            }

            var message = TokenParser.ParseText(data.MessageToShowIfConditionsFail);
            
            if (data.IsLetter)
            {
                Game1.drawLetterMessage(message);
            }
            else
            {
                Game1.drawObjectDialogue(message);
            }

            return false;
        }
        
        if (!Game1.player.mailReceived.Contains(data.FlagToSet))
        {
            if (!Game1.player.isInventoryFull())
            {
                if (string.IsNullOrWhiteSpace(data.PlaySound) == false)
                {
                    Game1.playSound(data.PlaySound);
                }

                foreach (var tileChange in data.TileRemovals)
                {
                    arg1.removeMapTile(tileChange.X, tileChange.Y, tileChange.Layer);
                }
                
                foreach (var tileChange in data.TileChanges)
                {
                    arg1.setMapTile(tileChange.X, tileChange.Y, tileChange.Index, tileChange.Layer, tileChange.TileSheetId);
                }

                foreach (var trigger in data.TriggerActions)
                {
                    if (TriggerActionManager.TryRunAction(trigger, out var error, out var exception) == false)
                    {
                        Log($"Error while running action: {error}. {exception}");
                    }
                }
                
                foreach (var item in data.Items)
                {
                    var createdItem = ItemRegistry.Create(item.QualifiedItemId, item.Stack, item.Quality);
                    if (createdItem is StardewValley.Object)
                    {
                        createdItem.IsRecipe = item.IsRecipe;
                    }
                    
                    Game1.player.addItemByMenuIfNecessaryElseHoldUp(createdItem);
                }
                
                Game1.player.mailReceived.Add(data.FlagToSet);
            }
            else
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(data.MessageToShowIfAlreadyReceived))
            {
                return false;
            }

            var messageIfAlreadyReceived = TokenParser.ParseText(data.MessageToShowIfAlreadyReceived);
            
            if (data.IsLetterReceived)
            {
                Game1.drawLetterMessage(messageIfAlreadyReceived);
            }
            else
            {
                Game1.drawObjectDialogue(messageIfAlreadyReceived);
            }
        }

        return true;
    }

    public static bool Question(GameLocation arg1, string[] arg2, Farmer arg3, Point arg4)
    { 
        if (arg2 is null || arg2.Length < 2 || ModEntry.QuestionTileAction.TryGetValue(arg2[1], out var data) == false)
            return false;

        if (GameStateQuery.CheckConditions(data.Condition) == false)
        {
            if (string.IsNullOrWhiteSpace(data.MessageToShowIfConditionsFail))
            {
                return false;
            }

            var message = TokenParser.ParseText(data.MessageToShowIfConditionsFail);
            
            if (data.IsLetter)
            {
                Game1.drawLetterMessage(message);
            }
            else
            {
                Game1.drawObjectDialogue(message);
            }

            return false;
        }

        var question = TokenParser.ParseText(data.Question);
        var responses = new Response[] { new("Yes", TokenParser.ParseText(data.Yes)), new("No", TokenParser.ParseText(data.No)) };
        arg1.createQuestionDialogue(question, responses, RunActions);
        return true;
        
        void RunActions(Farmer who, string whichanswer)
    	{
            if (whichanswer == "No")
            	return;
            
            foreach (var action in data.TriggerActions)
            {
                if (TriggerActionManager.TryRunAction(action, out var error, out var exception) == false)
                {
                    Log($"Error while running action: {error}. {exception}");
                }
            }
    	}
    }

    public static bool ConditionalWarp(GameLocation arg1, string[] arg2, Farmer arg3, Point arg4)
    { 
        //command dataName location x y
        if (arg2 is null || arg2.Length < 4 || ModEntry.ConditionalWarpTileAction.TryGetValue(arg2[1], out var data) == false)
            return false;

        if (GameStateQuery.CheckConditions(data.Condition) == false)
        {
            if (string.IsNullOrWhiteSpace(data.MessageToShowIfConditionsFail))
            {
                return false;
            }

            var message = TokenParser.ParseText(data.MessageToShowIfConditionsFail);
            
            if (data.IsLetter)
            {
                Game1.drawLetterMessage(message);
            }
            else
            {
                Game1.drawObjectDialogue(message);
            }

            return false;
        }

        if (string.IsNullOrWhiteSpace(data.Question) == false)
        {
            var question = TokenParser.ParseText(data.Question);
            var responses = new Response[] { new("Yes", TokenParser.ParseText(data.Yes)), new("No", TokenParser.ParseText(data.No)) };
            arg1.createQuestionDialogue(question, responses, WarpFarmer);
        }
        else
        {
            Game1.warpFarmer(arg2[2], int.Parse(arg2[3]), int.Parse(arg2[4]),false);
        }
        return true;
        
        void WarpFarmer(Farmer who, string whichanswer)
    	{
            if (whichanswer == "No")
            	return;
            
            Game1.warpFarmer(arg2[2], int.Parse(arg2[3]), int.Parse(arg2[4]),false);
    	}
    }
}