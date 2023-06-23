using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FarmVisitors
{
    internal static class Actions
    {
        #region normal visit
        //regular visit: the one used by non-scheduled NPCs
        internal static void AddToFarmHouse(NPC visitor, FarmHouse farmHouse, bool hadConfirmation)
        {
            try
            {
                if (!Values.IsVisitor(visitor.Name))
                {
                    ModEntry.Log($"{visitor.displayName} is not a visitor!", LogLevel.Trace);
                    return;
                }
                
                Game1.getCharacterFromName(visitor.Name).IsInvisible = true;
                
                if(hadConfirmation == false)
                {
                    DrawDialogue(visitor,Values.GetDialogueType(visitor,DialogueType.Introduce));
                }

                var position = farmHouse.getEntryLocation();
                position.Y--;
                visitor.faceDirection(0);
                Game1.warpCharacter(visitor, farmHouse, position.ToVector2());

                visitor.showTextAboveHead(string.Format(Values.GetDialogueType(visitor, DialogueType.WalkIn),Game1.player.Name));

                //set before greeting because "Push" leaves dialogues at the top
                if (Game1.player.isMarried())
                {
                    if (ModEntry.Config.InLawComments is not "None")
                        InLawActions(visitor);
                }

                var enterDialogue = Values.GetDialogueType(visitor, DialogueType.Greet);
                var randomInt = Game1.random.Next(101);
                if (ModEntry.Config.GiftChance >= randomInt)
                {
                    var withGift = $"{enterDialogue}#$b#{Values.GetGiftDialogue(visitor)}";

                    if(ModEntry.Config.Debug)
                        ModEntry.Log($"withGift: {withGift}", LogLevel.Trace);

                    enterDialogue = string.Format(withGift, Values.GetSeasonalGifts());
                }

                visitor.setNewDialogue($"{enterDialogue}", true, true);
                visitor.CurrentDialogue.Push(new Dialogue(Values.GetDialogueType(visitor, DialogueType.Thanking),visitor));

                if (Game1.currentLocation.Equals(farmHouse))
                {
                    Game1.currentLocation.playSound("doorClose");
                }

                position.Y--;
                visitor.controller = new PathFindController(visitor, farmHouse, position, 0);
            }
            catch(Exception ex)
            {
                ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
            }

        }
        internal static void Retire(NPC c, FarmHouse farmHouse)
        {
            var currentLocation = Game1.currentLocation;
            var inFarm = FarmOutside.NPCinScreen();

            if (currentLocation.Equals(farmHouse) || inFarm)
            {
                try
                {
                    if (c.controller is not null)
                    {
                        c.Halt();
                        c.controller = null;
                    }
                    Game1.fadeScreenToBlack();
                }
                catch (Exception ex)
                {
                    ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
                }
                finally
                {
                    DrawDialogue(c, Values.GetDialogueType(c, DialogueType.Retiring));
                    ReturnToNormal(c);
                    if (!inFarm)
                    {
                        Game1.currentLocation.playSound("doorClose");
                    }
                }
            }
            else
            {
                try
                {
                    Leave(c);
                }
                catch (Exception ex)
                {
                    ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
                }
            }
        }
        private static void InLawActions(NPC visitor)
        {
            var addedAlready = false;
            var name = visitor.Name;

            if (!ModEntry.Config.ReplacerCompat && Moddeds.IsVanillaInLaw(name))
            {
                if (Vanillas.InLawOfSpouse(name))
                {
                    visitor.setNewDialogue(Vanillas.GetInLawDialogue(name), true);
                    addedAlready = true;
                }
            }

            if (ModEntry.Config.InLawComments is "VanillaAndMod" || ModEntry.Config.ReplacerCompat)
            {
                var spouse = Moddeds.GetRelativeName(name);
                if (spouse is not null && !addedAlready)
                {
                    var formatted = string.Format(Moddeds.GetDialogueRaw(), spouse);
                    visitor.setNewDialogue(formatted, true);
                    addedAlready = true;
                }
            }

            if (Game1.player.getChildrenCount() > 0 && addedAlready)
            {
                visitor.setNewDialogue(Vanillas.AskAboutKids(Game1.player), true);
            }
        }
        #endregion

        #region custom
        // customized visits: ones set by user via ContentPatcher
        internal static void AddCustom(NPC c, FarmHouse farmHouse, ScheduleData data, bool hadConfirmation)
        {
            try
            {
                if (!Values.IsVisitor(c.Name))
                {
                    ModEntry.Log($"{c.displayName} is not a visitor!", LogLevel.Trace);
                    return;
                }

                var npcv = new DupeNPC(c);

                if(hadConfirmation == false)
                {
                    DrawDialogue(npcv,
                        !string.IsNullOrWhiteSpace(data.EntryQuestion)
                            ? data.EntryQuestion
                            //Game1.drawDialogue(npcv, Values.GetIntroDialogue(npcv));
                            : Values.GetDialogueType(npcv, DialogueType.Introduce));
                }
                var position = farmHouse.getEntryLocation();
                position.Y--;
                npcv.faceDirection(0);
                Game1.warpCharacter(npcv, farmHouse, position.ToVector2());

                npcv.showTextAboveHead(!string.IsNullOrWhiteSpace(data.EntryBubble)
                    ? string.Format(data.EntryBubble, Game1.player.Name)
                    : string.Format(Values.GetDialogueType(npcv, DialogueType.WalkIn), Game1.player.Name));

                if (!string.IsNullOrWhiteSpace(data.EntryDialogue))
                {
                    npcv.setNewDialogue(data.EntryDialogue, true);
                }
                else
                {
                    var enterDialogue = Values.GetDialogueType(npcv, DialogueType.Greet);
                    var randomInt = Game1.random.Next(101);
                    if (ModEntry.Config.GiftChance >= randomInt)
                    {
                        enterDialogue += "#$b#" + Values.GetGiftDialogue(npcv);
                        enterDialogue = string.Format(enterDialogue, Values.GetSeasonalGifts());
                    }
                    npcv.setNewDialogue(enterDialogue,true);
                }

                if (Game1.currentLocation.Equals(farmHouse))
                {
                    Game1.currentLocation.playSound("doorClose");
                }

                position.Y--;
                npcv.controller = new PathFindController(npcv, farmHouse, position, 0);
            }
            catch(Exception ex)
            {
                ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
            }

        }
        internal static void RetireCustom(NPC c, FarmHouse farmHouse, string text)
        {
            if (Game1.currentLocation.Equals(farmHouse))
            {
                try
                {
                    if(c.controller is not null)
                    {
                        c.Halt();
                        c.controller = null;
                    }
                    Game1.fadeScreenToBlack();
                }
                catch (Exception ex)
                {
                    ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
                }
                finally
                {
                    DrawDialogue(c, text);

                    ReturnToNormal(c);
                    Game1.currentLocation.playSound("doorClose");
                }
            }
            else
            {
                try
                {
                    Leave(c);
                }
                catch (Exception ex)
                {
                    ModEntry.Log($"An error ocurred when pathing to entry: {ex}", LogLevel.Error);
                }
            }
        }
        internal static void AddWhileOutside(NPC visitor)
        {
            try
            {
                var farmHouse = Utility.getHomeOfFarmer(Game1.player);

                if (!Values.IsVisitor(visitor.Name))
                {
                    ModEntry.Log($"{visitor.displayName} is not a visitor!", LogLevel.Trace);
                    return;
                }

                var position = farmHouse.getEntryLocation();
                position.Y--;
                visitor.faceDirection(0);
                Game1.warpCharacter(visitor, farmHouse, position.ToVector2());

                visitor.showTextAboveHead(string.Format(Values.GetDialogueType(visitor, DialogueType.WalkIn), Game1.player.Name));

                position.Y--;
                visitor.controller = new PathFindController(visitor, farmHouse, position, 0);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Error while adding to farmhouse: {ex}", LogLevel.Error);
            }
        }
        #endregion

        #region used by both

        private static void DrawDialogue(NPC visitor, string text) => Game1.drawDialogue(visitor, text);

        private static void ReturnToNormal(Character c)
        {
            var name = c.Name;
            //ModEntry.Visitor.Removed();
            ModEntry.Visitor = null;
            var realNpc = Game1.getCharacterFromName(name, false);
            realNpc.IsInvisible = false;
        }
       /*
        private static void RemoveAnimation(NPC npcv)
        {
            try
            {
                ModEntry.Log($"Stopping animation for {npcv.displayName} and resizing...", LogLevel.Trace);

                npcv.Sprite.StopAnimation();
                npcv.Sprite.SpriteWidth = 16;
                npcv.Sprite.SpriteHeight = 32;
                npcv.reloadSprite();

                npcv.Sprite.CurrentFrame = 0;
                npcv.faceDirection(0);

                if (npcv.endOfRouteMessage.Value is not null)
                {
                    npcv.endOfRouteMessage?.Value.Remove(0);
                    npcv.endOfRouteMessage.Value = null;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Error while stopping {npcv.displayName} animation: {ex}", LogLevel.Error);
            }
        }  
        */
       private static void Leave(Character c)
        {
            if (c.controller is not null)
            {
                c.Halt();
                c.controller = null;
            }
            Game1.drawObjectDialogue(string.Format(Values.GetNpcGone(Game1.currentLocation.Name.StartsWith("Cellar")), c.displayName));
            ReturnToNormal(c);
        }
       
/*
        public static Point GetRandomTile(GameLocation location, NPC who, int tries = 30, int maxDistance = 6)
        {
            var zero = Point.Zero;
            for (var i = 0; i < tries; i++)
            {
                var zerov = location.getRandomTile();
                zero = zerov.ToPoint();
                var canGetHere = IsPlaceable(zerov, location);

                //if the new point is too far away
                Point difference = new (Math.Abs(zero.X - (int)who.Position.X), Math.Abs(zero.Y - (int)who.Position.Y));
                if (difference.X > maxDistance || difference.Y > maxDistance)
                    continue;

                //if not too far away, check that there's no building
                if (IsThereABuilding(new PathFindController(who, location, zero, 1).pathToEndPoint, location))
                    canGetHere = false;

                if (canGetHere)
                {
                    break;
                }
            }

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"New position for {ModEntry.VisitorName}: {zero.X},{zero.Y}", LogLevel.Debug);
            }

            return zero;
        }

        public static PathFindController GetRandomTile(GameLocation location, NPC who, Random r, int tries = 30, int maxDistance = 6)
        {
            PathFindController path = null;
            var zero = Point.Zero;
            for (var i = 0; i < tries; i++)
            {
                var zerov = location.getRandomTile();
                zero = zerov.ToPoint();
                var canGetHere = IsPlaceable(zerov, location);

                //if the new point is too far away
                Point difference = new (Math.Abs(zero.X - (int)who.Position.X), Math.Abs(zero.Y - (int)who.Position.Y));
                if (difference.X > maxDistance || difference.Y > maxDistance)
                    continue;

                path = new PathFindController(who, location, zero, r.Next(0,4));

                //if not too far away, check that there's no building
                if (IsThereABuilding(path.pathToEndPoint, location))
                    canGetHere = false;

                if (canGetHere)
                {
                    break;
                }
            }

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"New position for {ModEntry.VisitorName}: {zero.X},{zero.Y}", LogLevel.Debug);
            }

            return path;
        }

        private static bool IsThereABuilding(Stack<Point> pathToEndPoint, GameLocation location)
        {
            var buildings = location.Map.GetLayer("Buildings");
            var back = location.Map.GetLayer("Back");

            foreach (var point in pathToEndPoint)
            {
                var tilebuildings = buildings.Tiles[point.X, point.Y];
                var tileback = back.Tiles[point.X, point.Y];
                var isBackgroundTile = IsBackground(tilebuildings) || IsBackground(tileback);

                if (tilebuildings != null || tileback == null || isBackgroundTile)
                    return true;
            }

            return false;
        }

        private static bool IsBackground(Tile tile) =>
            tile?.TileIndex == 0 && tile?.TileSheet.ImageSource is "Maps/townInterior" or "Maps/farmhouse_tiles";

        private static bool IsPlaceable(Vector2 asVector, GameLocation location) => location.CanLoadPathObjectHere(asVector) && !location.isTileOccupied(asVector) && location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(asVector); //&& location.canPetWarpHere(asVector);
        */
        /*
         private static bool IsPlaceable(Point asPoint, Vector2 asVector, GameLocation location) => location.CanLoadPathObjectHere(asVector) && location.isTileOnMap(asVector) && location.isTilePlaceable(asVector) && !location.isTileOccupied(asVector) && Utility.canGrabSomethingFromHere(asPoint.X, asPoint.Y, Game1.player);
        

        internal static Point GetRandomTile(GameLocation location, Random r, int tries = 30, int maxDistance = 6)
        {
            var who = Game1.getCharacterFromName(ModEntry.VisitorName);

            var map = location.map;

            var zero = Point.Zero;
            var canGetHere = false;

            for (var i = 0; i < tries; i++)
            {
                //we get random position using width and height of map
                zero = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));

                var isFloorValid = location.isTileOnMap(zero.ToVector2()) && location.isTilePassable(new Location(zero.X,zero.Y),Game1.viewport) && !location.isWaterTile(zero.X,zero.Y);
                var warpOrDoor = location.isCollidingWithWarpOrDoor(new Rectangle(zero, new Point(1, 1)));

                //check that location is clear + not water tile + not behind tree + not a warp
                canGetHere = location.isTileLocationTotallyClearAndPlaceable(zero.X, zero.Y) && !isFloorValid && warpOrDoor == null;

                //if the new point is too far away
                Point difference = new (Math.Abs(zero.X - (int)who.Position.X), Math.Abs(zero.Y - (int)who.Position.Y));
                if (difference.X > maxDistance || difference.Y > maxDistance)
                {
                    canGetHere = false;
                }

                if (canGetHere)
                {
                    break;
                }
            }

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"New position for {ModEntry.VisitorName}: {zero.X},{zero.Y}", LogLevel.Debug);
            }

            return zero;
        }

        private static bool IsBackgroundOrEmpty(GameLocation location, Point zero)
        {
            int[] validFloors = new int[]
            {
                336, 337, 352, 353
            };

            //index can be from 336 to 359, name has to be walls_and_floors
            var index = location.getTileIndexAt(zero, "Back");
            var sheetname = location.getTileSheetIDAt(zero.X, zero.Y, "Back");

            bool isIndexValid = validFloors.Contains(index);
            bool isExactSheet = sheetname == "walls_and_floors";
            
            if(!isExactSheet)
            {
                return location.Map.GetLayer("Back").Tiles[zero.X, zero.Y].TileSheet != null;
            }
            else
            {
                return isIndexValid && isExactSheet;
            }
        }*/
        #endregion

        public static void GoToSleep(NPC who)
        {
            var home = Utility.getHomeOfFarmer(Game1.player);
            if (!who.currentLocation.Equals(home))
            {
                Game1.warpCharacter(who,home,home.getEntryLocation().ToVector2());
            }
            
            if (Game1.currentLocation.Equals(home))
                Game1.currentLocation.playSound("doorClose");

            who.controller = new PathFindController(
                who,
                home,
                Values.GetBedSpot(),
                1,
                DoSleep(who)
            );
        }

        private static PathFindController.endBehavior DoSleep(NPC who)
        {
            who.playSleepingAnimation();
            return null;
        }
    }
}