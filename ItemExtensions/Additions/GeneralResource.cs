using ItemExtensions.Events;
using ItemExtensions.Models;
using ItemExtensions.Models.Enums;
using ItemExtensions.Models.Items;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Internal;
using StardewValley.Locations;
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

    internal static readonly string[] VanillaClumps = { "600", "602", "622", "672", "752", "754", "756", "758" };
    private static readonly int[] VanillaStones =
    {
        2, 4, 6, 8, 10, 12, 14, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 75, 76, 77, 95, 290, 343, 390, 450, 668, 670, 751, 760, 762, 764, 765, 25, 816, 817, 818, 819, 843, 844, 845, 846, 847, 849, 850
    };

    private static readonly int[] VanillaTwigs = { 294, 295 };

    private static readonly int[] VanillaWeeds =
    {
        0, 313, 314, 315, 316, 317, 318, 319, 320, 321, 452, 674, 675, 676, 677, 678, 679, 750, 784, 785, 786, 792, 793, 794, 882, 883, 884
    };

    internal static List<int> VanillaIds
    {
        get
        {
            var all = new List<int>();
            all.AddRange(VanillaStones);
            all.AddRange(VanillaTwigs);
            all.AddRange(VanillaWeeds);
            return all;
        }
    }
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    /// <summary>
    /// Compares tool requirements.
    /// </summary>
    /// <param name="tool">Tool used</param>
    /// <param name="data">Data holding required tool</param>
    /// <returns>Whether the aforementioned match. If tool is null (ie bomb), it's always true.</returns>
    internal static bool ToolMatches(Tool tool, ResourceData data)
    {
        if(string.IsNullOrWhiteSpace(data?.Tool))
        {
            Log("Resource's tool can't be null. Resource won't be mined.", LogLevel.Warn);
            return false;
        }

        #if DEBUG
        var toolName = tool?.GetToolData()?.ClassName;
        if (tool is MeleeWeapon debugWeapon)
            toolName = debugWeapon.DisplayName + " (weapon)";
        Log($"Tool: {toolName}, required: {data.Tool}");
        #endif
        
        //bombs call with null tool for Clumps
        if (tool is null)
            return true;

        //if any
        if (data.Tool.Equals("Any", IgnoreCase) || data.Tool.Equals("All", IgnoreCase))
        {
#if DEBUG
            Log("Any tool/weapon allowed");
#endif
            return true;
        }
        
        //"tool" → any non-weapon
        if (data.Tool.Equals("Tool", IgnoreCase))
            return tool is not MeleeWeapon;

        //for custom exceptions
        if (data.Tool.StartsWith("AnyExcept", IgnoreCase))
        {
            var exception = data.Tool.Remove(0, 9);
            
            return exception.Equals(data.Tool, IgnoreCase) == false;
        }
        
        //check weapon type
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

        if (tool is Pickaxe && data.Tool.Equals("pick", IgnoreCase))
        {
            return true;
        }
        
        //else, compare values
        var className = tool.GetToolData().ClassName;
#if DEBUG
        Log($"Tool: {className}, required: {data.Tool}, matches? {className.Equals(data.Tool, IgnoreCase)}");
#endif
        return className.Equals(data.Tool, IgnoreCase);
    }

    private static void CreateRadialDebris(GameLocation location, string debrisType, int xTile, int yTile, int numberOfChunks, bool resource, bool item = false, int quality = 0)
    {
        var vector = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
        var tileLocation = new Vector2(xTile, yTile);
        
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
            //get color
            var split = ArgUtility.SplitBySpaceQuoteAware(debrisType);
            ArgUtility.TryGet(split, 0, out var debrisName, out _);
            ArgUtility.TryGetOptional(split, 1, out var rawColor, out _, "White");
            var color = Utility.StringToColor(rawColor) ?? Color.White;
            var spawned = true;
            
            switch (debrisName.ToLower())
            {
                //default of debris
                case "iridium":
                    Game1.createRadialDebris(Game1.currentLocation, 10, xTile, yTile, Game1.random.Next(2, 4), false, color: color);
                    break;
                case "gold":
                    Game1.createRadialDebris(Game1.currentLocation, 6, xTile, yTile, Game1.random.Next(2, 4), false, color: color);
                    break;
                case "iron":
                    Game1.createRadialDebris(Game1.currentLocation, 2, xTile, yTile, Game1.random.Next(2, 4), false, color: color);
                    break;
                case "copper":
                    Game1.createRadialDebris(Game1.currentLocation, 0, xTile, yTile, Game1.random.Next(2, 4), false, color: color);
                    break;
                case "coal":
                    Game1.createRadialDebris(Game1.currentLocation, 4, xTile, yTile, Game1.random.Next(2, 4), false, color: color);
                    break;
                case "coins":
                    Game1.createRadialDebris(Game1.currentLocation, 8, xTile, yTile, Game1.random.Next(2, 7), false, color: color);
                    break;
                case "stone":
                    Game1.createRadialDebris(Game1.currentLocation, 14, xTile, yTile, Game1.random.Next(2, 7), false, color: color);
                    break;
                case "bigstone":
                case "boulder":
                    Game1.createRadialDebris(Game1.currentLocation, 32, xTile, yTile, Game1.random.Next(5, 11), false, color: color);
                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(48, tileLocation * 64f, color, 5, animationInterval: 180f, sourceRectWidth: 128, sourceRectHeight: 128)
                    {
                        alphaFade = 0.01f
                    });
                    break;
                case "wood":
                    Game1.createRadialDebris(Game1.currentLocation, 12, xTile, yTile, Game1.random.Next(2, 7), false, color: color);
                    break;
                case "bigwood":
                case "stump":
                    var textureStump = color == Color.White ? "TileSheets\\animations" : $"Mods/{ModEntry.Id}/Textures/Stump";
                    var sourceStump = new Rectangle(385, 1522, sbyte.MaxValue, 79);
                    if (color != Color.White)
                        sourceStump.Y = 0;
                    
                    Game1.createRadialDebris(Game1.currentLocation, 34, xTile, yTile, Game1.random.Next(5, 11), false, color: color);
                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(textureStump, sourceStump, 2000f, 1, 1, tileLocation * 64f + new Vector2(0.0f, 49f), false, false, 1E-05f, 0.016f, color, 1f, 0.0f, 0.0f, 0.0f));
                    break;
                case "custom":
                    /* follows this format:
                     * custom color texturepath x y width height frames [speed] [alphaFade]
                     */
                    ArgUtility.TryGetOptional(split, 2, out var textureSheet, out _, "TileSheets/animations");
                    ArgUtility.TryGetInt(split, 3, out var x, out var error);
                    ArgUtility.TryGetInt(split, 4, out var y, out error);
                    ArgUtility.TryGetInt(split, 5, out var width, out error);
                    ArgUtility.TryGetInt(split, 6, out var height, out error);
                    ArgUtility.TryGetInt(split, 7, out var frames, out error);
                    ArgUtility.TryGetOptionalFloat(split, 8, out var speed, out _, 180f);
                    ArgUtility.TryGetOptionalFloat(split, 9, out var alphaFade, out _, 0.01f);

                    if (string.IsNullOrWhiteSpace(error))
                    {
                        Log($"Error when creating custom animation: {error}", LogLevel.Error);
                        return;
                    }
                    
                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(textureSheet, new Rectangle(x, y, width, height), tileLocation, false, alphaFade, color){ animationLength = frames, interval = speed });
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
                    Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(28, vector * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), color, flipped: Game1.random.NextBool(), animationInterval: Game1.random.Next(60, 100)));
                }
                else if (debrisType.StartsWith("hay", IgnoreCase) || debrisType.StartsWith("grass", IgnoreCase))
                {
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

    private static void CreateItemDebris(string itemId, int howMuchDebris, int xTile, int yTile, GameLocation where, int quality = 0) => CreateRadialDebris(where, itemId, xTile, yTile, howMuchDebris, true, quality > 0, quality);

    private static void AddHay(ResourceData resource, GameLocation location, Vector2 tileLocation)
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
            layerDepth = (float)(1.0 - Game1.random.Next(100) / 10000.0),
            delayBeforeAnimationStart = Game1.random.Next(350)
        });
    }

    public static void CheckDrops(ResourceData resource, GameLocation location, Vector2 tileLocation, Tool t, bool bomb = false)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        Log("Checking resource drops...", LogLevel.Debug);
        
        var who = t?.getLastFarmerToUse() ?? Game1.player;

        if (resource.OnDestroy != null)
        {
            ActionButton.CheckBehavior(resource.OnDestroy, (int)tileLocation.X, (int)tileLocation.Y, location);
        }
        
        //create notes
        if (resource.SecretNotes && location.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.05)
        {
            var unseenSecretNote = location.tryToCreateUnseenSecretNote(who);
            
            if (unseenSecretNote != null)
                Game1.createItemDebris(unseenSecretNote, tileLocation * 64f, -1, location);
        }
            
        var num2 = Game1.random.Next(resource.MinDrops, resource.MaxDrops);
            
        if(!string.IsNullOrWhiteSpace(resource.Debris))
            CreateRadialDebris(Game1.currentLocation, resource.Debris, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(1, 6), false);

        if (!string.IsNullOrWhiteSpace(resource.ItemDropped))
        {
            //if a vanilla ore
            if (IsVanillaOre(resource.ItemDropped) && HasBuff(who , "dwarfStatue_0"))
            {
                num2++;
            }
            else if (IsGem(resource.ItemDropped) && Game1.player.stats.Get(StatKeys.Mastery(3)) > 0)
            {
                num2 *= 2;
            }

            CreateItemDebris(resource.ItemDropped, num2, (int)tileLocation.X, (int)tileLocation.Y, location);
        }

        if (resource.ExtraItems != null && resource.ExtraItems.Any())
        {
            TryExtraDrops(resource.ExtraItems, location, who, tileLocation);
        }

        if(!string.IsNullOrWhiteSpace(resource.BreakingSound) && bomb == false)
            location.playSound(resource.BreakingSound, tileLocation);

        if (resource.AddHay > 0)
        {
            AddHay(resource, location, tileLocation);
        }
        
        if(resource.ActualSkill >= 0)
            who.gainExperience(resource.ActualSkill, resource.Exp);

        if (resource.CountTowards is not StatCounter.None)
            AddStats(resource.CountTowards);

        if (location is not MineShaft shaft) 
            return;
        
        if (shaft.ladderHasSpawned || shaft.mineLevel == 77377)
            return;
        
        if (Game1.random.NextDouble() < ModEntry.Config.ChanceForStairs || IsLastNode(shaft))
            shaft?.createLadderDown((int)tileLocation.X, (int)tileLocation.Y);
    }

    private static bool IsLastNode(MineShaft shaft)
    {
        if (shaft.Objects.Length != 1) 
            return false;
#if DEBUG
        Log("Is last node on mineshaft.");
#endif
        return true;

    }

    /// <summary>
    /// Uses item IDs to check for geode data.
    /// </summary>
    /// <param name="item">Item id.</param>
    /// <returns>Whether the item is a geode.</returns>
    /// See <see cref="Utility.IsGeode(Item, bool)"/>
    private static bool IsGeode(string item)
    {
        if(string.IsNullOrWhiteSpace(item))
            return false;
        
        if (!item.Contains("MysteryBox"))
        {
            if (Game1.objectData.TryGetValue(item, out var value))
            {
                if (!value.GeodeDropsDefaultItems)
                {
                    List<ObjectGeodeDropData> geodeDrops = value.GeodeDrops;
                    if (geodeDrops == null)
                    {
                        return false;
                    }

                    return geodeDrops.Count > 0;
                }

                return true;
            }

            return false;
        }
        return false;
    }

    private static bool IsGem(string item)
    {
        if(string.IsNullOrWhiteSpace(item))
            return false;
        
        var data = ItemRegistry.GetData(item);

        if (data is null)
            return false;
        
        return data.Category == -2;
    }

    private static bool IsVanillaOre(string item)
    {
        if(string.IsNullOrWhiteSpace(item))
            return false;
        
        //both qualified and unqualified: copper, iron, gold, iridium and radioactive
        var ores = new[] { "378", "380", "384", "386", "909", "(O)378", "(O)380", "(O)384", "(O)386", "(O)909" };

        return ores.Contains(item);
    }

    private static void TryExtraDrops(IEnumerable<ExtraSpawn> data, GameLocation location, Farmer who, Vector2 tileLocation, int multiplier = 1)
    {
        var geodeChanceMultiplier = HasBuff(who ?? Game1.player,"dwarfStatue_4") ? 1.25 : 1.0;
        var addedCoalChance = HasBuff(who ?? Game1.player, "dwarfStatue_2") ? 0.1 : 0.0;

        foreach (var item in data)
        {
            var chance = Game1.random.NextDouble();

            //if coal, add coal chance
            if (item.ItemId is "(O)382" or "382")
                chance += addedCoalChance;

            //if geode, multiply chance
            if (IsGeode(item.ItemId))
                chance = chance * geodeChanceMultiplier;

            if(GameStateQuery.CheckConditions(item.Condition, location, who) == false)
                continue;
                
            //if it has a condition, check first
            if (chance > item.Chance)
                continue;

            Log($"Chance and condition match. Spawning extra item(s)...({item.ItemId})");
                
            var context = new ItemQueryContext(location, who, Game1.random, "ItemExtensions' TryExtraDrops");
            var itemQuery = ItemQueryResolver.TryResolve(item, context, item.Filter, item.AvoidRepeat);
            foreach (var result in itemQuery)
            {
#if DEBUG
                Log($"({item.ItemId}) Query item: {result.Item.QualifiedItemId}");
#endif
                    
                var parsedItem = ItemRegistry.Create(result.Item.QualifiedItemId, result.Item.Stack, result.Item.Quality);
                parsedItem.Stack *= multiplier;

#if DEBUG
                Log($"Parsed item: {parsedItem?.DisplayName} ({parsedItem?.QualifiedItemId})");
#endif
                if (IsVanillaOre(parsedItem.QualifiedItemId) && HasBuff(who, "dwarfStatue_0"))
                {
                    parsedItem.Stack++;
                }
                else if (IsGem(parsedItem.QualifiedItemId) && Game1.player.stats.Get(StatKeys.Mastery(3)) > 0)
                {
                    parsedItem.Stack *= 2;
                }

                var x = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                var y = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                var vector = new Vector2((int)tileLocation.X, (int)tileLocation.Y) * 64;
                location.debris.Add(new Debris(parsedItem,vector, vector + new Vector2(x, y)));
            }
        }
    }

    private static bool HasBuff(Farmer who, string buff)
    {
        try
        {
            return who.hasBuff(buff);
        }
        catch (Exception)
        {
            return false;
        }
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

    public static bool ShouldShowWrongTool(Tool tool, ResourceData resource)
    {
        if (resource.SayWrongTool is null || resource.SayWrongTool.HasValue == false)
            return false;
        
        return resource.SayWrongTool switch
        {
            NotifyForTool.None => false,
            NotifyForTool.Tool when tool is not MeleeWeapon => true,
            NotifyForTool.Weapon when tool is MeleeWeapon => true,
            NotifyForTool.All => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets weapon damage to resource.
    /// </summary>
    /// <param name="tool">Tool</param>
    /// <param name="damage">Damage as fallback, if tool is null</param>
    /// <param name="useEasierCalc">Whether to use node calculations instead of clumps'.</param>
    /// <returns>A damage calculation that depends on tool Type</returns>
    public static int GetDamage(Tool tool, int damage, bool useEasierCalc)
    {
        var mult = useEasierCalc ? 1f : 0.75f;
        
        //if weapon, get 10% of avg.
        if (tool is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            var wpnDmg = middle / 10;

            //some weapons have very little damage- if so, give them damage 1.
            if (wpnDmg <= 0)
                return 1;
            else
                return wpnDmg;
        }
        //if no tool, return fallback
        if (tool is null)
        {
            return damage;
        }
        //otherwise, return calculation
        var dmg = (int)Math.Max(1f, (tool.UpgradeLevel + 1) * mult);
        
        if (tool is Pickaxe p)
        {
            dmg += p.additionalPower.Value;
        }
        if (tool is Axe a)
        {
            dmg += a.additionalPower.Value;
        }

        return dmg;
    }

    public static bool IsVanilla(string id)
    {
        if (int.TryParse(id, out var asInt))
        {
            //if it's a vanilla ID
            if (asInt < 1000)
                return true;
        }

        return false;
    }
}