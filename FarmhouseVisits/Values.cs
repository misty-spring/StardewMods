using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using lv = StardewModdingAPI.LogLevel;

namespace FarmVisitors
{
    public enum ItemType
    {
        Animal,
        Crop,
        Furniture
    }

    public enum DialogueType
    {
        //about the visit
        Introduce,
        WalkIn,
        Furniture,
        Greet,
        Retiring,
        Thanking,
        Rejected,

        //about the farm
        Animal,
        Crop,
        NoneYet,
        Winter
    }
    internal static class Values
    {
        internal static bool IsVisitor(string c)
        {
            return c.Equals(ModEntry.Visitor.Name);
        }
        
        /// <summary>
        /// Checks if a NPC is free to visit player.
        /// </summary>
        /// <param name="who">NPC to check</param>
        /// <param name="isRandom">If it's a randomly-chosen visit.</param>
        /// <returns></returns>
        internal static bool IsFree(NPC who, bool isRandom)
        {
            //if character is robin and there's a construction ongoing
            if (who.Name == "Robin" && (Game1.getFarm().isThereABuildingUnderConstruction() || who.currentLocation.Equals(Game1.getFarm())))
            {
                ModEntry.Log(
                    $"{who.displayName} is building in the Farm. Visit will be cancelled.", 
                    lv.Trace);
                return false;
            }

            var isAnimating = who.doingEndOfRouteAnimation.Value;
            var isInvisible = who.IsInvisible;
            var isHospitalDay = Utility.IsHospitalVisitDay(who.Name);
            var visitedToday = ModEntry.TodaysVisitors.Contains(who.Name);
            var visitingIsland = Game1.IsVisitingIslandToday(who.Name);
            var isSleeping = who.isSleeping.Value;

            var defaultReqs = !isHospitalDay && !visitingIsland && !isSleeping && !isAnimating && !isInvisible;

            if (visitedToday && !isRandom)
            {
                ModEntry.Log(
                    $"{who.displayName} has already visited the Farm today!",
                    lv.Trace);
            }
            if (isHospitalDay)
            {
                ModEntry.Log(
                    $"{who.displayName} has a hospital visit scheduled today. They won't visit the farmer.", 
                    lv.Trace);
            }
            if (visitingIsland)
            {
                ModEntry.Log(
                    $"{who.displayName} is visiting the island today!",
                    lv.Trace);
            }
            if (isSleeping)
            {
                ModEntry.Log(
                    $"{who.displayName} is sleeping right now.",
                    lv.Trace);
            }

            if (isRandom)
            {
                return !visitedToday && defaultReqs;
            }

            return defaultReqs;
        }
        
        /// <summary>
        /// Retrieves a dialogue of the specified type.
        /// </summary>
        /// <param name="c">The NPC whose info to use.</param>
        /// <param name="which">The type of dialogue.</param>
        /// <returns></returns>
        internal static string GetDialogueType(NPC c, DialogueType which)
        {
            //allowed values: Introduce, Rejected, Greet, Retiring, WalkIn, Furniture
            
            var r = Game1.random.Next(1, 4);
            var type = Game1.random.Next(0, 3);
            
            if(which == DialogueType.Retiring && ModEntry.Config.UniqueDialogue)
            {
                var r2 = Game1.random.Next(0,3);
                if(r2 == 2)
                {
                    return ModEntry.TL.Get($"NPC{which}.{c.Name}.{r}");
                }
            }
            
            if (type is 0)
            {
                if (c.SocialAnxiety.Equals(0))
                {
                    return ModEntry.TL.Get($"NPC{which}.Outgoing{r}");
                }

                return ModEntry.TL.Get($"NPC{which}.Shy{r}");
            }

            if (c.Manners.Equals(1)) //polite
            {
                return ModEntry.TL.Get($"NPC{which}.Polite{r}");
            }

            if (c.Manners.Equals(2)) //rude
            {
                return ModEntry.TL.Get($"NPC{which}.Rude{r}");
            }

            //neutral
            return ModEntry.TL.Get($"NPC{which}.Neutral{r}");
        }

        /// <summary>
        /// Returns dialogue related to gifting.
        /// </summary>
        /// <param name="c">Character whose data is used.</param>
        /// <returns></returns>
        internal static string GetGiftDialogue(NPC c)
        {
            var r = Game1.random.Next(1, 6);

            if(Game1.random.Next(0,4).Equals(1))
            {
                if (c.SocialAnxiety.Equals(0))
                {
                    return ModEntry.TL.Get($"NPCGift.Normal{r}");
                }

                return ModEntry.TL.Get($"NPCGift.Kind{r}");
            }

            if (c.Manners.Equals(1)) //polite
            {
                return ModEntry.TL.Get($"NPCGift.Kind{r}");
            }

            if (c.Manners.Equals(2)) //rude
            {
                return ModEntry.TL.Get($"NPCGift.Lax{r}");
            }

            return ModEntry.TL.Get($"NPCGift.Normal{r}");
        }
        
        /// <summary>
        /// Returns translation string, for when NPC leaves and player isn't home.
        /// </summary>
        /// <param name="isCellar">Whether the player is in the Cellar.</param>
        /// <returns></returns>
        internal static string GetNpcGone(bool isCellar)
        {
            if(isCellar)
            {
                return ModEntry.TL.Get("NPCGone_Cellar");
            }

            return ModEntry.TL.Get("NPCGoneWhileOutside");
        }
        
        /// <summary>
        /// Return gift IDs depending on season.
        /// </summary>
        /// <returns></returns>
        internal static string GetSeasonalGifts()
        {
            const string defaultgifts = "419 246 423 216 247 688 176 180 436 174 182 184 424 186 438"; //Vinegar, Wheat Flour, Rice, Bread, Oil, Warp Totem: Farm, small Egg (2 types),Large Egg (2 types), Goat Milk, Milk, Cheese, Large Milk, L. Goat Milk
            if (Game1.IsSpring)
            {
                return $"{defaultgifts} 400 252 222 190 610 715"; //Strawberry, Rhubarb, Rhubarb Pie, Cauliflower, Fruit Salad, Lobster
            }

            if (Game1.IsSummer)
            {
                return $"{defaultgifts} 636 256 258 270 690 260"; //Peach, Tomato, Blueberry, Corn, Warp Totem: Beach, Hot Pepper
            }

            if (Game1.IsFall)
            {
                return $"{defaultgifts} 276 284 282 395 689 613"; //Pumpkin, Beet, Cranberries, Coffee, Warp Totem: Mountains, Apple
            }

            if (Game1.IsWinter)
            {
                return $"{defaultgifts} 414 144 147 178 787 440"; //Crystal Fruit, Pike, Herring, Hay, Battery Pack, Wool
            }

            return defaultgifts;
        }
        
        /// <summary>
        /// Get a random index from the player's furniture list.
        /// </summary>
        /// <returns></returns>
        internal static string GetRandomObj(ItemType i)
        {

            var list = i switch
            {
                ItemType.Furniture => ModEntry.FurnitureList,
                ItemType.Animal => ModEntry.Animals,
                ItemType.Crop => ModEntry.Crops.Values.ToList(),
                _ => throw new KeyNotFoundException()
            };

            //if there's no furniture, return null
            if (list is null)
            {
                return null;
            }

            //choose random index and return itsdisplayname
            var amount = list.Count;
            var r = Game1.random.Next(0, (amount + 1));
            return list[r];
        }
        
        /// <summary>
        /// Updates furniture list.
        /// </summary>
        /// <param name="farmHouse">The farmhouse whose data to use.</param>
        /// <returns></returns>
        internal static List<string> UpdateFurniture(FarmHouse farmHouse)
        {
            List<string> templist = new();
            foreach (var f in farmHouse.furniture)
            {
                //templist.Add(f.DisplayName);
                templist.Add($"\"{f.DisplayName.ToLower()}\"");
            }
            return templist;
        }
        
        /// <summary>
        /// Gets the index and name of all crop types in Farm.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<int, string> GetCrops()
        {
            try
            {
                //var slash = ModEntry.Slash;
                var cropData = Game1.content.Load<Dictionary<int, string>>("Data/Crops");
                var objData = Game1.content.Load<Dictionary<int, string>>("Data/ObjectInformation");

                Dictionary<int, string> tempdict = new();
                var farm = Game1.getFarm();

                foreach (var value in farm.terrainFeatures.Values)
                {
                    var isDirt = value is HoeDirt;
                    if (!isDirt) //if not dirt
                        continue;

                    var crop = (value as HoeDirt).crop;

                    if (crop == null || crop.dead.Value) continue;
                    var index = crop.rowInSpriteSheet.Value != Crop.rowOfWildSeeds ? crop.netSeedIndex.Value : crop.whichForageCrop.Value;

                    if(ModEntry.Config.Debug)
                    {
                        ModEntry.Log($"crop index: {index}", lv.Info);
                    }

                    if (tempdict.ContainsKey(index))
                        continue;

                    string seedinfo = null;
                    cropData?.TryGetValue(index, out seedinfo);
                    if (seedinfo == null)
                    {
                        if (ModEntry.Config.Debug)
                        {
                            ModEntry.Log($"Key {index} not found in CropData.", lv.Warn);
                        }
                        continue;

                    }

                    //var objInd = SpanSplit.GetNthChunk(info, slash, 3).ToString(); <- not working for some reason
                    var seedSplit = seedinfo.Split('/');
                    /*if (ModEntry.Debug)
                        {
                            ModEntry.Log($"seedSplit = {seedSplit}", lv.Info);
                        }*/

                    var objInd = seedSplit[3];

                    if (ModEntry.Config.Debug)
                    {
                        ModEntry.Log($"CropData Key= {index}; string info = {seedinfo}; objInd= {objInd}", lv.Info);
                    }

                    objData.TryGetValue(int.Parse(objInd), out var objInfo);
                    //var name = SpanSplit.GetNthChunk(objInfo, slash, 5).ToString();
                    if (objInfo == null) continue;
                    var objSplit = objInfo.Split('/');
                    var name = objSplit[4];

                    //tempdict.Add(index, $"\"{name}\"");
                    tempdict.Add(index, $"{name}");
                }
                return tempdict;
            }
            catch(Exception ex)
            {
                ModEntry.Log($"Error while getting crops: {ex}", lv.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Returns name of all animals.
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetAnimals()
        {
            var all = Game1.getFarm().getAllFarmAnimals();

            List<string> templist = new();
            foreach (var animal in all)
            {
                templist.Add($"\"{animal.Name}\"");
            }
            return templist;
        }

        public static bool IsCloseToSleepTime(NPC who)
        {
            var time = Game1.timeOfDay;
            foreach (var data in who.Schedule)
            {
                if (data.Key <= time) continue;
                if (data.Key - time > 100) continue;
                
                if (who.Schedule[data.Key].endOfRouteBehavior.Contains("sleep")) 
                    return true;
            }

            return false;
        }

        public static Point GetBedSpot(BedFurniture.BedType bedType = BedFurniture.BedType.Any)
        {
            return GetBed(bedType)?.GetBedSpot() ?? new Point(-1000, -1000);
        }
        private static BedFurniture GetBed(BedFurniture.BedType bedType = BedFurniture.BedType.Any, int index = 0)
        {
            //Furniture f in IslandFarmHouse.Object
            foreach (var f in Utility.getHomeOfFarmer(Game1.player).furniture)
            {
                if (f is not BedFurniture bed) continue;
                if (bedType != BedFurniture.BedType.Any && bed.bedType != bedType) continue;
                if (index == 0)
                {
                    return bed;
                }
                index--;
            }
            return null;
        }
    }
}