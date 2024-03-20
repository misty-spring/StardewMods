using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;

namespace ItemExtensions.Additions;

/// <summary>
/// Methods used both by resource clumps and nodes.
/// </summary>
public static class GeneralResource
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    /// <summary>
    /// Compares tool requirements.
    /// </summary>
    /// <param name="tool">Tool used</param>
    /// <param name="data">Data holding required tool</param>
    /// <returns>Whether the aforementioned match. If tool is null (ie bomb), it's always true.</returns>
    internal static bool ToolMatches(Tool tool, ResourceData data)
    {
        if (tool is null) //bomb
            return true;

        if (data.Tool.Equals("Any", IgnoreCase) || data.Tool.Equals("All", IgnoreCase))
            return true;
        
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
                //default of debris
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

            var isCustom = !spawned;
            if (isCustom)
            {
                //these can be of "debris" or "debris #color"
                if (debrisType.StartsWith("weeds", IgnoreCase))
                {
                    var split = ArgUtility.SplitBySpace(debrisType);
                    var color = Utility.StringToColor(split[1]) ?? Color.White;
                    Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(28, vector * 64f + new Vector2((float) Game1.random.Next(-16, 16), (float) Game1.random.Next(-16, 16)), color, flipped: Game1.random.NextBool(), animationInterval: (float) Game1.random.Next(60, 100)));
                }
                else if (debrisType.StartsWith("hay", IgnoreCase) || debrisType.StartsWith("grass", IgnoreCase))
                {
                    var split = ArgUtility.SplitBySpace(debrisType);
                    var color = Utility.StringToColor(split[1]) ?? Color.White;
                    
                    //grass bits
                    Game1.createRadialDebris(location, color == Color.White ? "TerrainFeatures/grass" : $"Mods/{ModEntry.Id}/Textures/Grass", new Rectangle(2, 8, 8, 8), 1, (int)(vector.X * 64),(int)(vector.Y * 64), Game1.random.Next(6, 14), (int) vector.Y + 1, color, 4f);
                }
                else
                {
                    isCustom = false;
                }
            }

            if (spawned || isCustom)
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

    internal static void AddHay(ResourceData resource, GameLocation location, Vector2 tileLocation)
    {
        //store hay
        GameLocation.StoreHayInAnySilo(resource.AddHay, location);
                
        //hay icon above head
        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, tileLocation - new Vector2(0.0f, 128f), false, false, Game1.player.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0.0f, 0.0f)
        {
            motion =
            {
                Y = -1f
            },
            layerDepth = (float)(1.0 - (double)Game1.random.Next(100) / 10000.0),
            delayBeforeAnimationStart = Game1.random.Next(350)
        });
    }

    public static void CheckDrops(ResourceData resource, GameLocation location, Vector2 tileLocation, Tool t)
    {
        //create notes
        if (resource.SecretNotes && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
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
                if(GameStateQuery.CheckConditions(item.Condition, location, t.getLastFarmerToUse()) == false)
                    continue;
                
                //if it has a condition, check first
                if (chance > item.Chance)
                    continue;

                Log("Chance and condition match. Spawning extra item(s)...");
                
                var context = new ItemQueryContext(location, t.getLastFarmerToUse(), Game1.random);
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

        if (resource.AddHay > 0)
        {
            AddHay(resource, location, tileLocation);
        }
        
        if(resource.ActualSkill >= 0)
            t.getLastFarmerToUse().gainExperience(resource.ActualSkill, resource.Exp);

        if (resource.CountTowards is not StatCounter.None)
            AddStats(resource.CountTowards);
    }

    private static void AddStats(StatCounter stat)
    {
        switch (stat)
        {
            case StatCounter.Copper:
                Game1.player.stats.CopperFound++;
                break;
            case StatCounter.Diamonds:
                Game1.player.stats.DiamondsFound++;
                break;
            case StatCounter.GeodesBroken: 
                Game1.player.stats.GeodesCracked++;
                break;
            case StatCounter.Gold: 
                Game1.player.stats.GoldFound++;
                break;
            case StatCounter.Iridium: 
                Game1.player.stats.IridiumFound++;
                break;
            case StatCounter.Iron: 
                Game1.player.stats.IronFound++;
                break;
            case StatCounter.MysticStones:
                Game1.player.stats.MysticStonesCrushed++;
                break;
            case StatCounter.OtherGems:
                Game1.player.stats.OtherPreciousGemsFound++;
                break;
            case StatCounter.PrismaticShards:
                Game1.player.stats.PrismaticShardsFound++;
                break;
            case StatCounter.Stone:
                Game1.player.stats.StoneGathered++;
                break;
            case StatCounter.Stumps:
                Game1.player.stats.StumpsChopped++;
                break;
            case StatCounter.Seeds:
                Game1.player.stats.SeedsSown++;
                break;
            case StatCounter.Weeds:
                Game1.player.stats.WeedsEliminated++;
                break;
            case StatCounter.None:
            default:
                break;
        }
    }
}