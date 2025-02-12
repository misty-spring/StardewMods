using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Items;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Internal;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Monsters;
using xTile.Dimensions;

namespace ItemExtensions.Additions;

public static class Sorter
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    private static Stack<Monster> MonsterQueue { get; set; } = new();
    
    internal static int GetMaxFeatures(int level)
    {
        if (level % 20 == 0)
            return 0;

        if (level == 77377)
            return 15;

        var remainder = level % 30;
        return remainder;
    }
    /// <summary>
    /// Grabs all ores that match a random double. (E.g, all with a chance bigger than 0.x, starting from smallest)
    /// </summary>
    /// <param name="randomDouble"></param>
    /// <param name="canApply"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal static IList<string> GetAllForThisDouble(double randomDouble, Dictionary<string, double> canApply)
    {
        try
        {
            var validEntries = new Dictionary<string, double>();
            foreach (var (id, chance) in canApply)
            {
                //e.g. if randomDouble is 0.56 and this ore's chance is 0.3, it'll be skipped
                if (randomDouble > chance)
                    continue;
                validEntries.Add(id, chance);
            }

            if (validEntries.Any() == false)
                return ArraySegment<string>.Empty;

            //turns sorted to list. we do this instead of calculating directly because IOrdered has no indexOf, and I'm too exhausted to think of something better (perhaps optimize in the future)
            var convertedSorted = GetAscending(validEntries);

            var result = new List<string>();
            for (var i = 0; i < convertedSorted.Count; i++)
            {
                result.Add(convertedSorted[i]);
                if (i + 1 >= convertedSorted.Count)
                    break;
                
                var current = convertedSorted[i];
                var next = convertedSorted[i + 1];
#if DEBUG
                Log($"Added {convertedSorted[i]} node to list.");
#endif

                //if next one has higher %
                //because doubles are always a little off, we do a comparison of difference
                if (Math.Abs(validEntries[next] - validEntries[current]) > 0.0000001)
                {
                    break;
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Log($"Error while sorting spawn chances: {e}.\n  Will be skipped.", LogLevel.Warn);
            return new List<string>();
        }
    }
    
    /// <summary>
    /// Sorts by less chance to bigger.
    /// </summary>
    /// <param name="data">Data to sort.</param>
    /// <returns>A list with only the IDs.</returns>
    private static List<string> GetAscending(Dictionary<string, double> data)
    {
        //sorts by smallest to biggest
        var sorted = from entry in data orderby entry.Value select entry;
        
        var result = new List<string>();
        foreach (var pair in sorted)
        {
            result.Add(pair.Key);
#if DEBUG
            Log($"Added {pair.Key} to sorted list ({pair.Value})");
#endif
        }

        return result;
    }

    internal static bool GetItem(ExtraSpawn data, ItemQueryContext context, out Item item)
    {
        item = null;
        
        try
        {
            if (data.Chance < Game1.random.NextDouble())
                return false;

            //if there's a condition AND it doesn't match
            if (string.IsNullOrWhiteSpace(data.Condition) == false && GameStateQuery.CheckConditions(data.Condition, context.Location, context.Player) == false)
                return false;

            var avoidItemIds = new HashSet<string>(data.AvoidItemIds);
            
            var solvedQuery = ItemQueryResolver.TryResolve(data, context, data.Filter, data.AvoidRepeat, avoidItemIds);

            var chosenItem = solvedQuery.FirstOrDefault()?.Item;

            if (string.IsNullOrWhiteSpace(chosenItem?.QualifiedItemId))
                return false;

            item = ItemRegistry.Create(chosenItem.QualifiedItemId, chosenItem.Stack, data.Quality);
            return true;
        }
        catch(Exception ex)
        {
            Log($"Exception while sorting item query: {ex}.", LogLevel.Warn);
            return false;
        }
    }

    public static Monster GetMonster(MonsterSpawnData monster, Vector2 tile, int facing)
    {
        var name = monster.Name.ToLower().Replace(" ", "");
        Monster mon = name switch
        {
            "bat" => new Bat(tile, 0),
            "frostbat" => new Bat(tile, 40),
            "lavabat" => new Bat(tile, 80),
            "iridiumbat" => new Bat(tile, 171),
            "doll" or "curseddoll" => new Bat(tile, -666),
            "skull" or "hauntedskull" => new Bat(tile, 77377),
            "magmasprite" => new Bat(tile, -555),
            "magmasparker" => new Bat(tile, -556),
            "bigslime" or "biggreenslime" => new BigSlime(tile, 0),
            "bigblueslime" => new BigSlime(tile, 40),
            "bigredslime" => new BigSlime(tile, 80),
            "bigpurpleslime" => new BigSlime(tile, 121),
            "bluesquid" => new BlueSquid(tile),
            "bug" => new Bug(tile, 0),
            "armoredbug" => new Bug(tile, 121),
            "dino" or "dinomonster" or "pepper" or "pepperrex" or "rex" => new DinoMonster(tile),
            "duggy" => new Duggy(tile),
            "magmaduggy" => new Duggy(tile, true),
            "dust" or "sprite" or "dustsprite" or "spirit" or "dustspirit" => new DustSpirit(tile),
            "dwarvishsentry" or "dwarvish" or "sentry" => new DwarvishSentry(tile),
            "ghost" => new Ghost(tile),
            "carbonghost" => new Ghost(tile, "Carbon Ghost"),
            "putridghost" => new Ghost(tile, "Putrid Ghost"),
            "slime" or "greenslime" => new GreenSlime(tile, 0),
            "blueslime" => new GreenSlime(tile, 40),
            "redslime" => new GreenSlime(tile, 80),
            "purpleslime" => new GreenSlime(tile, 121),
            "tigerslime" => new GreenSlime(tile, 0),
            "prismaticslime"=> new GreenSlime(tile, 0),
            "grub" or "cavegrub" => new Grub(tile, false),
            "fly" or "cavefly" => new Fly(tile, false),
            "mutantgrub" => new Grub(tile, true),
            "mutantfly" => new Fly(tile, true),
            "metalhead" => new MetalHead(tile, 0),
            "hothead" => new HotHead(tile),
            "lavalurk" => new LavaLurk(tile),
            "mummy" => new Mummy(tile),
            "rockcrab" => new RockCrab(tile),
            "lavacrab" => new RockCrab(tile, "Lava Crab"),
            "iridiumcrab" => new RockCrab(tile, "Iridium Crab"),
            "falsemagmacap" or "magmacap" => new RockCrab(tile, "False Magma Cap"),
            "stickbug" => new RockCrab(tile),
            "trufflecrab" => new RockCrab(tile, "Truffle Crab"),
            "rockgolem" or "stonegolem" or "wildernessgolem" => new RockGolem(tile),
            "iridiumgolem" => new RockGolem(tile, Game1.player.CombatLevel),
            "serpent" => new Serpent(tile),
            "royalserpent" => new Serpent(tile, "Royal Serpent"),
            "brute" or "shadowbrute" => new ShadowBrute(tile),
            "shaman" or "shadowshaman" => new ShadowShaman(tile),
            "sniper" or "shadowsniper" => new Shooter(tile),
            "skeleton" => new Skeleton(tile),
            "skeletonmage" or "mage" => new Skeleton(tile, true),
            "spider" or "leaper" => new Leaper(tile),
            "spiker" => new Spiker(tile, facing),
            "squidkid" => new SquidKid(tile),
            _ => new Monster(monster.Name, tile, facing),
        };

        //disable ranged attack if applies, turn specific monsters
        switch (name)
        {
            case "shadowsniper":
            case "sniper":
                if(monster.RangedAttacks == false)
                    ((Shooter)mon).nextShot = float.MaxValue;
                break;
            case "shaman":
            case "shadowshaman":
                if(monster.RangedAttacks == false)
                    ModEntry.Help.Reflection.GetField<int>(mon, "coolDown", false).SetValue(int.MaxValue);
                break;
            case "squid":
            case "squidkid":  
                if(monster.RangedAttacks == false)
                    ModEntry.Help.Reflection.GetField<int>(mon, "lastFireball", false).SetValue(int.MaxValue);
                break;         
            case "pepper":
            case "pepperrex":
            case "pepper rex":
            case "rex":
                if (monster.RangedAttacks == false)
                {
                    ((DinoMonster)mon).nextFireTime = int.MaxValue;
                    ((DinoMonster)mon).timeUntilNextAttack = int.MaxValue;
                }
                break;
            case "stickbug":
                (mon as RockCrab)?.makeStickBug();
                break;
            case "tigerslime" or "tiger":
                ((GreenSlime)mon).makeTigerSlime();
                break;
            case "prismaticslime":
                ((GreenSlime)mon).makePrismatic();
                break;
            case "wildernessgolem":
                break;
        }

        if (monster.FollowPlayer == false)
        {
            var spotted = ModEntry.Help.Reflection.GetField<bool>(mon, "spottedPlayer", false);
            spotted.SetValue(false);
            mon.IsWalkingTowardPlayer = false;
        }

        if (monster.GracePeriod > 0)
        {
            MonsterQueue.Push(mon);
            Game1.delayedActions.Add(new DelayedAction(monster.GracePeriod, AttackPlayer));
        }
        
        if (monster.HideShadow || name.Contains("magma") || name.Contains("truffle"))
        {
            mon.HideShadow = true;
        }
        
        
        if (monster.Color.HasValue && (name.Contains("slime") || name.Contains("hothead"))) //if it's a slime or hothead, color is used
        {
            ((GreenSlime)mon).color.Value = monster.Color.Value;
        }
        
        if(monster.Hardmode)
            mon.isHardModeMonster.Set(true);
        
        if (monster.Health > 0)
        {
            mon.MaxHealth = monster.Health;
            mon.Health = monster.Health;
        }

        return mon;
    }

    private static void AttackPlayer()
    {
        if (MonsterQueue.Count <= 0)
            return;

        var who = MonsterQueue.Pop();

        var spotted = ModEntry.Help.Reflection.GetField<bool>(who, "spottedPlayer", false);
        spotted.SetValue(false);
        who.IsWalkingTowardPlayer = false;
    }

    public static TriggerActionData GetTriggerAction(string id)
    {
        var triggerActions = Game1.content.Load<List<TriggerActionData>>("Data/TriggerActions");
        foreach (var triggerData in triggerActions)
        {
            if (triggerData.Id.Equals(id))
                return triggerData;
        }
        return null;
    }
}