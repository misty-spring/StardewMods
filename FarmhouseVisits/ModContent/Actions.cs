using FarmhouseVisits.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace FarmhouseVisits.ModContent;

internal static class Actions
{
    private static LogLevel Level => ModEntry.Level;
    
    #region normal visit
    //regular visit: the one used by non-scheduled NPCs
    internal static void AddToFarmHouse(NPC who, FarmHouse farmHouse, bool hadConfirmation)
    {
        try
        {
            if (!Values.IsVisitor(who.Name))
            {
                ModEntry.Log($"{who.displayName} is not a visitor!", Level);
                return;
            }
            
            HaltEverything(who);
            #if DEBUG
            ModEntry.Log("Current dialogues:" + who.CurrentDialogue?.Count, Level);
            #endif
            
            if (hadConfirmation == false)
            {
                DrawDialogue(who, Values.GetDialogueType(who, DialogueType.Introduce));
            }

            var position = farmHouse.getEntryLocation();
            position.Y--;
            who.faceDirection(0);
            Game1.warpCharacter(who, farmHouse, position.ToVector2());

            var textBubble = string.Format(Values.GetDialogueType(who, DialogueType.WalkIn), Game1.player.Name);
            who.showTextAboveHead(textBubble);

            //set before greeting because "Push" leaves dialogues at the top
            if (Game1.player.isMarriedOrRoommates())
            {
                if (ModEntry.Config.InLawComments is not "None")
                    InLawActions(who);
            }

            var text = Values.GetDialogueType(who, DialogueType.Greet);
            var randomInt = Game1.random.Next(101);
            if (ModEntry.Config.GiftChance >= randomInt)
            {
                var withGift = $"{text}#$b#{Values.GetGiftDialogue(who)}";

                if (ModEntry.Config.Debug)
                    ModEntry.Log($"withGift: {withGift}", Level);

                text = string.Format(withGift, Values.GetSeasonalGifts());
            }
            
            //more lines, but fixes index bug
            who.CurrentDialogue = new Stack<Dialogue>();
            
            SetDialogue(who, text);
            PushDialogue(who, Values.GetDialogueType(who, DialogueType.Thanking));

            if (Game1.player.currentLocation.Equals(farmHouse))
            {
                Game1.player.currentLocation.playSound("doorClose");
            }

            position.Y--;
            who.controller = new PathFindController(who, farmHouse, position, 0);
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
        }
    }

    internal static void Retire(NPC who)
    {
        CheckSafeShutdown();
        
        var currentLocation = Game1.player.currentLocation;
        var visible = Values.NpcOnScreen(who, currentLocation);

        //if same as npc AND not farm
        if (currentLocation.Equals(who.currentLocation))
        {
            try
            {
                if (who.controller != null)
                {
                    who.Halt();
                    who.controller = null;
                }
                Game1.fadeScreenToBlack();
            }
            catch (Exception ex)
            {
                ModEntry.Log($"An error occurred when pathing to entry: {ex}", LogLevel.Error);
            }
            finally
            {
                DrawDialogue(who, Values.GetDialogueType(who, DialogueType.Retiring));
                ReturnToNormal(who);
                if (!visible)
                {
                    Game1.player.currentLocation.playSound("doorClose");
                }
            }
        }
        else
        {
            try
            {
                Leave(who);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
            }
        }
    }

    private static void InLawActions(NPC who)
    {
        var addedAlready = false;
        var name = who.Name;

        if (!ModEntry.Config.ReplacerCompat && Data.IsVanillaInLaw(name))
        {
            if (Data.InLawOf_vanilla(name))
            {
                SetDialogue(who, Data.GetInLawDialogue(name));
                addedAlready = true;
            }
        }

        if (ModEntry.Config.InLawComments is "VanillaAndMod" || ModEntry.Config.ReplacerCompat)
        {
            var spouse = Data.GetRelativeName(name);
            if (spouse is not null && !addedAlready)
            {
                var formatted = string.Format(Data.GetDialogueRaw(), spouse);
                SetDialogue(who, formatted);
                addedAlready = true;
            }
        }

        if (Game1.player.getChildrenCount() > 0 && addedAlready)
        {
            SetDialogue(who, Data.AskAboutKids(Game1.player));
        }
    }
    #endregion

    #region custom
    // customized visits: ones set by user via ContentPatcher
    internal static void AddCustom(NPC who, FarmHouse farmHouse, ScheduleData data, bool hadConfirmation)
    {
        try
        {
            if (!Values.IsVisitor(who.Name))
            {
                ModEntry.Log($"{who.displayName} is not a visitor!", Level);
                return;
            }

            if (hadConfirmation == false)
            {
                //if custom entry question, use that. if not, normal one
                string text;
                if (!string.IsNullOrWhiteSpace(data.EntryQuestion))
                    text = data.EntryQuestion;
                else
                    text = Values.GetDialogueType(who, DialogueType.Introduce);

                DrawDialogue(who, text);
            }

            //warp
            var position = farmHouse.getEntryLocation();
            position.Y--;
            who.faceDirection(0);
            Game1.warpCharacter(who, farmHouse, position.ToVector2());

            //if custom entry text, use that. if not, get normal text
            string textBubble;
            if (!string.IsNullOrWhiteSpace(data.EntryBubble))
                textBubble = string.Format(data.EntryBubble, Game1.player.Name);
            else
                textBubble = string.Format(Values.GetDialogueType(who, DialogueType.WalkIn), Game1.player.Name);

            who.showTextAboveHead(textBubble);

            //if has entry dialogue, set
            if (!string.IsNullOrWhiteSpace(data.EntryDialogue))
            {
                SetDialogue(who, data.EntryDialogue);
            }
            else
            {
                //otherwise, generic dialogue with % of gift
                var enterDialogue = Values.GetDialogueType(who, DialogueType.Greet);
                var randomInt = Game1.random.Next(101);
                if (ModEntry.Config.GiftChance >= randomInt)
                {
                    enterDialogue += "#$b#" + Values.GetGiftDialogue(who);
                    enterDialogue = string.Format(enterDialogue, Values.GetSeasonalGifts());
                }
                SetDialogue(who, enterDialogue);
            }

            if (Game1.player.currentLocation.Equals(farmHouse))
            {
                Game1.player.currentLocation.playSound("doorClose");
            }

            position.Y--;
            who.controller = new PathFindController(who, farmHouse, position, 0);
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
        }

    }

    internal static void RetireCustom(NPC who, string text)
    {
        if (Game1.player.currentLocation.Equals(who.currentLocation))
        {
            try
            {
                if (who.controller is not null)
                {
                    who.Halt();
                    who.controller = null;
                }
                Game1.fadeScreenToBlack();
            }
            catch (Exception ex)
            {
                ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
            }
            finally
            {
                DrawDialogue(who, text);

                ReturnToNormal(who);
                Game1.player.currentLocation.playSound("doorClose");
            }
        }
        else
        {
            try
            {
                Leave(who);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
            }
        }
    }

    internal static void AddWhileOutside(NPC who)
    {
        try
        {
            var farmHouse = Utility.getHomeOfFarmer(Game1.player);

            if (!Values.IsVisitor(who.Name))
            {
                ModEntry.Log($"{who.displayName} is not a visitor!", Level);
                return;
            }

            var position = farmHouse.getEntryLocation();
            position.Y--;
            who.faceDirection(0);
            Game1.warpCharacter(who, farmHouse, position.ToVector2());

            who.showTextAboveHead(string.Format(Values.GetDialogueType(who, DialogueType.WalkIn), Game1.player.Name));

            position.Y--;
            who.controller = new PathFindController(who, farmHouse, position, 0);
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
        }
    }
    #endregion

    #region used by both

    /// <summary>
    /// Create dialogue and push to front of stack.
    /// </summary>
    /// <param name="who"></param>
    /// <param name="text"></param>
    internal static void PushDialogue(NPC who, string text)
    {
        var dialogue = new Dialogue(who, null, text);
        who.CurrentDialogue.Push(dialogue);
    }

    /// <summary>
    /// Set dialogue 
    /// </summary>
    /// <param name="who"></param>
    /// <param name="text"></param>
    internal static void SetDialogue(NPC who, string text, bool add = true)
    {
        var dialogue = new Dialogue(who, null, text);
        who.setNewDialogue(dialogue, add);
    }

    /// <summary>
    /// Draws dialogue to screen.
    /// </summary>
    /// <param name="who"></param>
    /// <param name="text"></param>
    // Because this method doesn't exist anymore in Game1, we do its equiv.
    public static void DrawDialogue(NPC who, string text)
    {
        CheckSafeShutdown();
        
        var db = new Dialogue(who, null, text);
        who.CurrentDialogue.Push(db);
        Game1.drawDialogue(who);
    }

    private static void ReturnToNormal(NPC who)
    {
        var where = who.currentLocation;
        where.characters.Remove(who);

        ModEntry.SetNoVisitor();
    }

    private static void Leave(NPC who)
    {
        CheckSafeShutdown();
        
        if (who.controller is not null)
        {
            who.Halt();
            who.controller = null;
        }

        var exitMessage = string.Format(Values.GetNpcGone(Game1.player.currentLocation.Name.Equals("Cellar")),
            who.displayName);
        
        Game1.addHUDMessage(new HUDMessage(exitMessage, 2));

        ReturnToNormal(who);
    }

    public static void GoToSleep(NPC who, VisitData context)
    {
        var bed = Values.GetBedSpot();
        if (bed == Point.Zero)
        {
            ModEntry.Log($"Found no bed. Visit {who.Name} won't stay over.", LogLevel.Warn);
            Retire(who);
            return;
        }

        context.IsGoingToSleep = true;

        var home = Utility.getHomeOfFarmer(Game1.player);
        if (!who.currentLocation.Equals(home))
        {
            CheckSafeShutdown();
        
            Game1.fadeScreenToBlack();
            Game1.warpCharacter(who, home, home.getEntryLocation().ToVector2());

            if (Game1.player.currentLocation.Equals(home))
                Game1.player.currentLocation.playSound("doorClose");
            else
            {
                var rawtext = ModEntry.TL.Get("NPCGoneToSleep");
                var formatted = string.Format(rawtext, who.displayName);
                Game1.addHUDMessage(new HUDMessage(formatted, 2));
            }
        }

        who.doEmote(24, false);

        who.controller = new PathFindController(
            who,
            home,
            bed,
            1,
            DoSleep
        );
    }

    private static void DoSleep(Character who, GameLocation location)
    {
        if (who is not NPC npc)
            return;
        
        npc.playSleepingAnimation();
    }

    internal static void WalkAroundFarm(NPC who)
    {
        var where = Game1.getFarm();
        var newspot = Data.RandomTile(where, who, 15);

        if (newspot != Vector2.Zero)
        {
            who.temporaryController = null;
            who.controller = new PathFindController(
                who,
                where,
                newspot.ToPoint(),
                Game1.random.Next(0, 4)
                );

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"is the controller empty?: {who.controller == null}", LogLevel.Debug);
            }
        }

        if (Game1.random.Next(0, 11) <= 5)
        {
            var anyCrops = ModEntry.Crops.Any();

            if (Game1.currentSeason == "winter")
            {
                SetDialogue(who, Values.GetDialogueType(who, DialogueType.Winter));
            }
            else if ((Game1.random.Next(0, 2) <= 0 || !anyCrops) && ModEntry.Animals.Any())
            {
                var animal = Game1.random.ChooseFrom(ModEntry.Animals);
                var rawtext = Values.GetDialogueType(who, DialogueType.Animal);
                var formatted = string.Format(rawtext, animal);
                SetDialogue(who, formatted);
            }
            else if (anyCrops)
            {
                var crop = Game1.random.ChooseFrom(ModEntry.Crops);
                var rawtext = Values.GetDialogueType(who, DialogueType.Crop);
                var formatted = string.Format(rawtext, crop);
                SetDialogue(who, formatted);
            }
            else
            {
                SetDialogue(who, Values.GetDialogueType(who, DialogueType.NoneYet));
            }
        }
    }

    internal static void CheckSafeShutdown()
    {
        if (Game1.activeClickableMenu is null)
            return;

        var child = Game1.activeClickableMenu.GetChildMenu();
        if (child is not null)
        {
            var grandchild = child.GetChildMenu();
            grandchild?.exitThisMenu(false);
            
            child.exitThisMenu(false);
        }
        
        Game1.activeClickableMenu.exitThisMenu();
    }
    #endregion

    public static void HaltEverything(NPC npc)
    {
        npc.Halt();
        npc.controller = null;
        npc.temporaryController = null;
        npc.CurrentDialogue?.Clear();
        npc.CurrentDialogue ??= new Stack<Dialogue>();
        //npc.Dialogue?.Clear();
        
        if (npc.IsWalkingTowardPlayer)
            npc.IsWalkingTowardPlayer = false;
        
        if (npc.CurrentDialogue?.Count > 0 && npc.CurrentDialogue.Peek().removeOnNextMove && npc.Tile != npc.DefaultPosition / 64f)
        {
            npc.CurrentDialogue.Pop();
        }
    }
}