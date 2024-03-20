using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

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
        
        //#if DEBUG
        //Log("Button is valid!",LogLevel.Debug);
        //#endif

        if (Game1.player.ActiveObject == null)
            return;

        if (!ModEntry.Data.TryGetValue(Game1.player.ActiveObject.ItemId, out var data))
            return;

        if (data.OnUse == null)
            return;
        
        CheckBehavior(data.OnUse);
    }

    public static void CheckBehavior(OnBehavior behavior)
    {
        if (!GameStateQuery.CheckConditions(behavior.Condition))
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
                    RunBehavior(behavior, false);
            }

            Game1.currentLocation.createQuestionDialogue(TokenParser.ParseText(behavior.Message), responses, AfterDialogueBehavior);
        }
        else
        {
            RunBehavior(behavior);
        }
    }

    private static void RunBehavior(OnBehavior behavior, bool directAction = true)
    {
        #region items
        if (behavior.ReduceBy > 0)
            Game1.player.ActiveObject.ConsumeStack(behavior.ReduceBy);

        if (behavior.AddItems != null)
        {
            foreach (var pair in behavior.AddItems)
            {
                var item = ItemRegistry.Create(pair.Key, pair.Value);
                Game1.player.addItemByMenuIfNecessary(item);
            }
        }
        
        if (behavior.RemoveItems != null)
        {
            foreach (var pair in behavior.RemoveItems)
            {
                Log($"Removing {pair}...");
                Game1.player.removeFirstOfThisItemFromInventory(pair.Key, pair.Value);
            }
        }
        #endregion

        #region player values
        if (!string.IsNullOrWhiteSpace(behavior.Health))
        {
            Game1.player.health = ChangeValues(behavior.Health, Game1.player.health, Game1.player.maxHealth);
        }
        
        if (!string.IsNullOrWhiteSpace(behavior.Stamina))
        {
            Game1.player.Stamina = ChangeValues(behavior.Health, Game1.player.Stamina, Game1.player.MaxStamina);
        }

        if (!string.IsNullOrWhiteSpace(behavior.ChangeMoney))
        {
            Game1.player.Money = ChangeValues(behavior.ChangeMoney, Game1.player.Money, Game1.player.Money);
        }
        #endregion

        #region quests
        if(!string.IsNullOrWhiteSpace(behavior.AddQuest))
            Game1.player.addQuest(behavior.AddQuest);
        
        if(!string.IsNullOrWhiteSpace(behavior.AddSpecialOrder))
            Game1.player.team.AddSpecialOrder(behavior.AddSpecialOrder);
        
        if(!string.IsNullOrWhiteSpace(behavior.RemoveQuest))
            Game1.player.removeQuest(behavior.RemoveQuest);

        if (!string.IsNullOrWhiteSpace(behavior.RemoveSpecialOrder))
        {
            var specialOrders = Game1.player.team.specialOrders;
            for (var index = specialOrders.Count - 1; index >= 0; --index)
            {
                if (specialOrders[index].questKey.Value == behavior.RemoveSpecialOrder)
                    specialOrders.RemoveAt(index);
            }
        }
        #endregion
        
        #region menus
        /*if (!string.IsNullOrWhiteSpace(behavior.OpenMenu))
        {
            var newMenu = GetMenuType(behavior.OpenMenu);
            if(newMenu != null)
                Game1.activeClickableMenu = newMenu;
        }*/
        
        if(directAction && !string.IsNullOrWhiteSpace(behavior.Message))
        {
            Game1.addHUDMessage(new HUDMessage(TokenParser.ParseText(behavior.Message)));
        }

        if (behavior.ShowNote != null)
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
            else
            {
                ShowNote(note);
            }
        }
        #endregion
        
        #region play
        if (!string.IsNullOrWhiteSpace(behavior.PlaySound))
            Game1.playSound(behavior.PlaySound);
        
        if (!string.IsNullOrWhiteSpace(behavior.PlayMusic))
            Game1.changeMusicTrack(behavior.PlayMusic);
        #endregion

        if (string.IsNullOrWhiteSpace(behavior.TriggerAction)) 
            return;
        
        TriggerActionManager.TryRunAction(behavior.TriggerAction, out var error, out var exception);
        if (!string.IsNullOrWhiteSpace(error))
        {
            Log($"Error: {error}. {exception}");
        }
    }

    /// <summary>
    /// Shows a note.
    /// </summary>
    /// <param name="note">Note data.</param>
    /// <see cref="StardewValley.Menus.LetterViewerMenu"/>
    private static void ShowNote(NoteData note)
    {
        var menu = new LetterWithImage(note);
        Game1.activeClickableMenu = menu;
    }

    private static IClickableMenu GetMenuType(string which)
    {
        var split = which.Split(' ');
        var menu = split[0].ToLower();
        
        IClickableMenu result = menu switch
        {
            "forge" => new ForgeMenu(),
            "geode" => new GeodeMenu(),
            "billboard" => new Billboard(),
            "farmhand" => new FarmhandMenu(),
            "animal" or "animals" => new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock(Game1.currentLocation), Game1.currentLocation),
            "end" => new ShippingMenu(Game1.player.displayedShippedItems),
            "tailor" or "sew" or "sewing" => new TailoringMenu(),
            _ => null
        };

        return result;
    }

    private static int ChangeValues(string howMuch, float value, int defaultValue) =>
        ChangeValues(howMuch, (int)value, defaultValue);
    
    private static int ChangeValues(string howMuch, int value, int defaultValue)
    {
        if(string.IsNullOrWhiteSpace(howMuch))
            return -1;

        int result;
        
        if (int.TryParse(howMuch, out var justNumbers))
        {
            result = justNumbers <= 0 ? 1 : justNumbers;
            return result;
        }

        var split = howMuch.Split(' ');
        var type = split[0];
        var amt = int.Parse(split[1]);
        
        var addsOrReduces = type switch
        {
            "add" => true,
            "more" => true,
            "reduce" => true,
            "less" => true,
            "+" => true,
            "-" => true,
            _ => false
        };

        if(addsOrReduces)
        {
            Log("Adding/Substracting from player health.");

            //add/reduce hp
            if (type is "less" or "-" or "reduce")
            {
                var trueAmt = value - amt;
                result = trueAmt <= 0 ? 1 : trueAmt;
            }
            else
            {
                var trueAmt = value + amt;
                result = trueAmt >= value ? value : trueAmt;
            }
        }
        else if (type == "reset")
        {
            Log("Resetting player health.");
            result = defaultValue;
        }
        else
        {
            Log("Setting player health.");
            //set
            result = amt;
        }

        return result;
    }
}