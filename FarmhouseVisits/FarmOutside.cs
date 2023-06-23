using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using lv = StardewModdingAPI.LogLevel;

namespace FarmVisitors
{
    internal static class FarmOutside
    {
        internal static bool NPCinScreen()
        {
            var farm = Game1.getLocationFromName("Farm");

            var x = ((int)(ModEntry.Visitor.Position.X / 64));
            var y = ((int)(ModEntry.Visitor.Position.Y / 64));

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"farm name = {farm.Name}, visitor position = ({x}, {y})", lv.Info);
            }

            //return Utility.isOnScreen(who.Position.ToPoint(), 0, farm);
            return Utility.isOnScreen(new Point(x,y), 0, farm);
        }

        /* NOTE:
         * There is a NPC barrier surrounding the door. Characters warped to that tile won't be able to move.
         * For this reason, the NPC must be warped 2 tiles below the door.
         * 
         * This could be fixed by editing map properties- but it'd only be compatible with vanilla maps (and might have side effects). This is the best workaround currently.
         */
        internal static void PlayerWarp(object sender, WarpedEventArgs e)
        {
            if(ModEntry.ForcedSchedule)
            {
                return;
            }

            var isFarm = e.NewLocation.IsFarm;
            var isFarmHouse = e.NewLocation.Equals(Utility.getHomeOfFarmer(Game1.player));
            var isShed = e.NewLocation.NameOrUniqueName.Contains("Shed");

            if (!isFarm && !isFarmHouse && !isShed) //if its neither the farm nor the farmhouse
                return;

            if(ModEntry.Config.Debug)
               ModEntry.Log($"The new warp location is {e.NewLocation.NameOrUniqueName}",lv.Trace);
            
            if (!ModEntry.Config.WalkOnFarm || string.IsNullOrWhiteSpace(ModEntry.Visitor?.Name))
                return; //if npcs can't follow or there's no visit

            if (ModEntry.Config.Debug)
            {
                ModEntry.Log($"Leaving {e.OldLocation.Name}...Warped to {e.NewLocation.Name}. isFarm = {e.NewLocation.IsFarm} , CanFollow = {ModEntry.Config.WalkOnFarm}, VisitorName = {ModEntry.Visitor.Name}", lv.Info);
            }

            Point door;

            if (isShed) //if new location is shed
            {
                ModEntry.Visitor.IsOutside = false;
                door = Game1.player.Position.ToPoint();
            }
            else if (isFarm) //if new location is farm
            {
                ModEntry.Visitor.IsOutside = true;

                door = Utility.getHomeOfFarmer(Game1.player).getEntryLocation();
                //door.X--; //-1, moves npc one tile to the left
                door.Y += 2; //two more tiles down

            }
            else
            {
                ModEntry.Visitor.IsOutside = false;

                var home = Utility.getHomeOfFarmer(Game1.player);
                door = home.getEntryLocation();
            }

            var visit = Game1.getCharacterFromName(ModEntry.Visitor.Name);

            if (visit.controller is not null)
                visit.Halt();

            Game1.warpCharacter(visit,e.NewLocation,door.ToVector2());

            if (isFarm) //if new location is farm
            {
                visit.faceDirection(2);
                door.X--;
                visit.controller = new PathFindController(visit, Game1.getFarm(), door, 2);
            }
            else //if it's the farmhouse
            {
                visit.faceDirection(0);
                door.Y -= 2;
                door.X++;
                visit.controller = new PathFindController(visit, e.NewLocation, door, 0);
            }
        }

        internal static void WalkAroundFarm(NPC c)
        {
            var gameLocation = Game1.getFarm();
            //var newspot = getRandomOpenPointInFarm(gameLocation, Game1.random);
            var newspot = Actions.GetRandomTile(gameLocation);

            try
            {
                c.PathToOnFarm(newspot);
                
                if(ModEntry.Config.Debug)
                {
                    ModEntry.Log($"is the controller empty?: {c.controller == null}", lv.Debug);
                }
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Something went wrong (PathToOnFarm): {ex}", lv.Error);
            }

            if (Game1.random.Next(0, 11) <= 5)
            {
                var anyCrops = ModEntry.Crops.Any();

                if (Game1.currentSeason == "winter")
                {
                    c.setNewDialogue(
                        Values.GetDialogueType(
                            c,
                            DialogueType.Winter),
                        true);
                }
                else if ((Game1.random.Next(0, 2) <= 0 || !anyCrops) && ModEntry.Animals.Any())
                {
                    c.setNewDialogue(
                        string.Format(
                            Values.GetDialogueType(
                                c,
                                DialogueType.Animal),
                            Values.GetRandomObj(
                                ItemType.Animal)),
                        true);
                }
                else if (anyCrops)
                {
                    c.setNewDialogue(
                        string.Format(
                            Values.GetDialogueType(
                                c, 
                                DialogueType.Crop), 
                            Values.GetRandomObj(
                                ItemType.Crop)), 
                        true);
                }
                else
                {
                    c.setNewDialogue(
                        Values.GetDialogueType(
                            c,
                            DialogueType.NoneYet),
                        true);
                }
            }
        }
/*
        internal static Point GetRandomOpenPointInFarm(GameLocation location,Random r, int tries = 30, int maxDistance = 10)
        {
            var who = Game1.getCharacterFromName(ModEntry.Visitor.Name);

            var map = location.map;

            var zero = Point.Zero;
            var canGetHere = false;

            for (var i = 0; i < tries; i++)
            {
                //we get random position using width and height of map
                zero = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));

                var isFloorValid = location.isTileOnMap(zero.ToVector2()) && location.isTilePassable(new Location(zero.X, zero.Y), Game1.viewport) && !location.isWaterTile(zero.X, zero.Y);
                var isBehindTree = location.isBehindTree(zero.ToVector2());
                var warpOrDoor = location.isCollidingWithWarpOrDoor(new Rectangle(zero, new Point(1, 1)));

                //check that location is clear + not water tile + not behind tree + not a warp
                canGetHere = location.isTileLocationTotallyClearAndPlaceable(zero.X, zero.Y) && isFloorValid && !isBehindTree && warpOrDoor == null;

                //if the new point is too far away
                Point difference = new (Math.Abs(zero.X - (int)who.Position.X),Math.Abs(zero.Y - (int)who.Position.Y));
                if(difference.X > maxDistance && difference.Y > maxDistance)
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
                ModEntry.Log($"New position for {ModEntry.Visitor.Name}: {zero.X},{zero.Y}", lv.Debug);
            }
           
            return zero;
        }
*/
        internal static void DoFloodFill(GameLocation location, Point start)
        {
            /*
            If node is not Inside return.
            2. Set the node
            3. Perform Flood-fill one step to the south of node.
            4. Perform Flood-fill one step to the north of node
            5. Perform Flood-fill one step to the west of node
            6. Perform Flood-fill one step to the east of node
            7. Return.*/
            var tile = location.map.GetLayer("Buildings").Tiles[start.X, start.Y];
            if (tile != null) return;
            
            //if it doesn't exist, add
            if(!ModEntry.Locations.ContainsKey(location.NameOrUniqueName))
                ModEntry.Locations.Add(location.NameOrUniqueName, new List<Point>());
            
            if(!ModEntry.Locations[location.NameOrUniqueName].Contains(start))
                ModEntry.Locations[location.NameOrUniqueName].Add(start);
            
            DoFloodFill(location, new Point(start.X++, start.Y)); //1 step to right
            DoFloodFill(location, new Point(start.X--, start.Y)); //1 step to left
            DoFloodFill(location, new Point(start.X, start.Y++)); //1 step down
            DoFloodFill(location, new Point(start.X, start.Y--)); //1 step up
        }
        /*
         * later on try this?:
    fn fill(x, y):
    if not Inside(x, y) then return
    let s = new empty queue or stack
    Add (x, x, y, 1) to s
    Add (x, x, y - 1, -1) to s
    while s is not empty:
        Remove an (x1, x2, y, dy) from s
        let x = x1
        if Inside(x, y):
            while Inside(x - 1, y):
                Set(x - 1, y)
                x = x - 1
        if x < x1:
            Add (x, x1-1, y-dy, -dy) to s
        while x1 <= x2:
            while Inside(x1, y):
                Set(x1, y)
                x1 = x1 + 1
            Add (x, x1 - 1, y+dy, dy) to s
            if x1 - 1 > x2:
                Add (x2 + 1, x1 - 1, y-dy, -dy) to s
            x1 = x1 + 1
            while x1 < x2 and not Inside(x1, y):
                x1 = x1 + 1
            x = x1
         */
    }
}
