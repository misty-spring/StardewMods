using ItemExtensions.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public partial class ObjectPatches
{
    internal static void Postfix_performToolAction(Object __instance, Tool t)
    {
        if (ModEntry.Resources.TryGetValue(__instance.ItemId, out var resource) == false)
            return;

        if (!ToolMatches(t, resource))
            return;

        //set vars
        var location = __instance.Location;
        var tileLocation = __instance.TileLocation;
        var damage = GetDamage(t);
        
        //if temp data doesn't exist, it means we also have to set minutes until ready
        try
        {
            _ = __instance.tempData["Health"];
        }
        catch(Exception)
        {
            __instance.tempData ??= new Dictionary<string, object>();
            __instance.tempData.TryAdd("Health", resource.Health);
            __instance.MinutesUntilReady = resource.Health;
        }

        if (damage < resource.MinToolLevel)
        {
            location.playSound("clubhit", tileLocation);
            location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
            Game1.player.jitterStrength = 1f;
            return;
        }

        if(!string.IsNullOrWhiteSpace(resource.Sound))
            location.playSound(resource.Sound, tileLocation);
        
        __instance.MinutesUntilReady -= damage + 1;

        if (__instance.MinutesUntilReady <= 0.0)
        {
            //create notes
            DoResourceDrop(__instance, t, resource);
            
            Destroy(__instance);
            
            if(resource.ActualSkill >= 0)
                t.getLastFarmerToUse().gainExperience(resource.ActualSkill, resource.Exp);
            
            return;
        }
        __instance.shakeTimer = 100;
    }

    internal static void DoResourceDrop(Object o, Tool t, ResourceData resource)
    {
        var location = o.Location;
        var tileLocation = o.TileLocation;
        if (location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
        {
            var unseenSecretNote = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
            if (unseenSecretNote != null)
                Game1.createItemDebris(unseenSecretNote, tileLocation * 64f, -1, location);
        }
            
        var num2 = Game1.random.Next(resource.MinDrops, resource.MaxDrops);
            
        if(!string.IsNullOrWhiteSpace(resource.Debris))
            CreateRadialDebris(Game1.currentLocation, resource.Debris, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(1, 6), false);

        if (!string.IsNullOrWhiteSpace(resource.ItemDropped))
        {
            if (Game1.IsMultiplayer)
            {
                Game1.recentMultiplayerRandom = Utility.CreateRandom(tileLocation.X * 1000.0, tileLocation.Y);
                for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                    CreateItemDebris(resource.ItemDropped, num2, (int)tileLocation.X, (int)tileLocation.Y, location);
            }
            else
            {
                CreateItemDebris(resource.ItemDropped, num2, (int)tileLocation.X, (int)tileLocation.Y, location);
            }
        }

        var chance = Game1.random.NextDouble();
        if (resource.ExtraItems != null)
        {
            foreach (var item in resource.ExtraItems)
            {
                if(GameStateQuery.CheckConditions(item.Condition, o.Location, t.getLastFarmerToUse()) == false)
                    continue;
                
                //if it has a condition, check first
                if (chance > item.Chance)
                    continue;

                Log("Chance and condition mstch. Spawning extra item(s)...");
                
                var context = new ItemQueryContext(o.Location, t.getLastFarmerToUse(), Game1.random);
                var itemQuery = ItemQueryResolver.TryResolve(item, context, item.Filter, item.AvoidRepeat);
                foreach (var result in itemQuery)
                {
                    var parsedItem = ItemRegistry.Create(result.Item.QualifiedItemId, result.Item.Stack, result.Item.Quality);
                    var x = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                    var y = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                    var vector = new Vector2((int)tileLocation.X, (int)tileLocation.Y) * 64;
                    location.debris.Add(new Debris(parsedItem,vector, vector + new Vector2(x, y)));
                }
            }
        }

        if(!string.IsNullOrWhiteSpace(resource.BreakingSound))
            location.playSound(resource.BreakingSound, tileLocation);
    }
    private static void Destroy(Object o, bool onlySetDestroyable = false)
    {
        if (o.lightSource is not null)
        {
            var id = o.lightSource.Identifier;
            o.Location.removeLightSource(id);
        }    
        
        o.CanBeSetDown = true;
        o.CanBeGrabbed = true;
        o.IsSpawnedObject = true; //by default false IIRC
        
        if(onlySetDestroyable)
            return;
        
        //o.performRemoveAction();
        o.Location.removeObject(o.TileLocation,false);
        o = null;
    }

    internal static int GetDamage(Tool tool)
    {
        //if melee weapon, do 10% of average DMG
        if (tool is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            return middle / 10;
        }

        return tool.UpgradeLevel;
    }

    internal static bool ToolMatches(Tool tool, ResourceData data)
    {
        if (tool is MeleeWeapon w)
        {
            //if the user set a number, we assume it's a custom tool
            if (int.TryParse(data.Tool, out var number))
                return w.type.Value == number;
            
            //any wpn
            if (data.Tool.Equals("meleeweapon", IgnoreCase)  || data.Tool.Equals("weapon", IgnoreCase))
                return true;

            int weaponType;
            
            if (data.Tool.Equals("dagger", IgnoreCase))
                weaponType = 1;
            else if (data.Tool.Equals("club", IgnoreCase) || data.Tool.Equals("hammer", IgnoreCase))
                weaponType = 2;
            else if (data.Tool.Contains("slash", IgnoreCase))
                weaponType = 3;
            else if (data.Tool.Equals("sword", IgnoreCase))
                weaponType = 30;
            else
                weaponType = 0;

            if (weaponType == 30)
            {
                return w.type.Value is 0 or 3;
            }

            return weaponType == w.type.Value;
        }
        
        var className = tool.GetToolData().ClassName;
        return className.Equals(data.Tool, IgnoreCase);
    }

    internal static void CreateRadialDebris(GameLocation location, string debrisType, int xTile, int yTile, int numberOfChunks, bool resource, bool item = false, int quality = 0)
    {
        var vector = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
        
        if (item)
        {
            while (numberOfChunks > 0)
            {
                var vector2 = Game1.random.Next(4) switch
                {
                    0 => new Vector2(-64f, 0f),
                    1 => new Vector2(64f, 0f),
                    2 => new Vector2(0f, 64f),
                    _ => new Vector2(0f, -64f),
                };
                var item2 = ItemRegistry.Create(debrisType, 1, quality);
                location.debris.Add(new Debris(item2, vector, vector + vector2));
                numberOfChunks--;
            }
        }
        
        if (resource)
        {
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f)));
        }
        else
        {
            var spawned = true;
            
            switch (debrisType.ToLower())
            {
                case "coins":
                    Game1.createRadialDebris(Game1.currentLocation, 8, xTile, yTile, Game1.random.Next(2, 7), false);
                    break;
                case "stone":
                    Game1.createRadialDebris(Game1.currentLocation, 14, xTile, yTile, Game1.random.Next(2, 7), false);
                    break;
                case "bigstone":
                    Game1.createRadialDebris(Game1.currentLocation, 32, xTile, yTile, Game1.random.Next(5, 11), false);
                    break;
                case "wood":
                    Game1.createRadialDebris(Game1.currentLocation, 12, xTile, yTile, Game1.random.Next(2, 7), false);
                    break;
                case "bigwood":
                    Game1.createRadialDebris(Game1.currentLocation, 34, xTile, yTile, Game1.random.Next(5, 11), false);
                    break;
                default:
                    spawned = false;
                    break;
            }

            if (spawned)
                return;
            
            while (numberOfChunks > 0)
            {
                var vector2 = Game1.random.Next(4) switch
                {
                    0 => new Vector2(-64f, 0f),
                    1 => new Vector2(64f, 0f),
                    2 => new Vector2(0f, 64f),
                    _ => new Vector2(0f, -64f),
                };
                
                //create debris with item data
                var item2 = ItemRegistry.GetData(debrisType);
                var sourceRect = item2.GetSourceRect();
                var debris = new Debris(spriteSheet: item2.TextureName, sourceRect, 1, vector + vector2);
                
                //fix item shown
                debris.Chunks[0].xSpriteSheet.Value = sourceRect.X;
                debris.Chunks[0].ySpriteSheet.Value = sourceRect.Y;
                debris.Chunks[0].scale = 4f;
                
                //debris.debrisType.Set(Debris.DebrisType.CHUNKS);
                
                location.debris.Add(debris);
                numberOfChunks--;
            }
        }
    }

    internal static void CreateItemDebris(string itemId, int howMuchDebris, int xTile, int yTile, GameLocation where, int quality = 0) => CreateRadialDebris(where, itemId, xTile, yTile, howMuchDebris, true, quality > 0, quality);

    internal static void Pre_onExplosion(Object __instance, Farmer who)
    {
        if (!ModEntry.Resources.TryGetValue(__instance.ItemId, out var resource))
            return;

        if (resource == null)
            return;

        //var sheetName = ItemRegistry.GetData(data.ItemDropped).TextureName;
        var where = __instance.Location;
        var tile = __instance.TileLocation;
        var num2 = Game1.random.Next(resource.MinDrops, resource.MaxDrops + 1);

        if (string.IsNullOrWhiteSpace(resource.ItemDropped))
            return;
        
        if (Game1.IsMultiplayer)
        {
            Game1.recentMultiplayerRandom = Utility.CreateRandom(tile.X * 1000.0, tile.Y);
            for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                CreateItemDebris(resource.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        else
        {
            CreateItemDebris(resource.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        
        Destroy(__instance, true);
    }
}