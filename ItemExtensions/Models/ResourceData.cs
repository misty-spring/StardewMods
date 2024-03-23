using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Models;

/// <summary>
/// Resource info.
/// </summary>
/// See <see cref="StardewValley.GameData.Objects.ObjectData"/>
public class ResourceData
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;
    
    // Required
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Texture { get; set; }
    public int SpriteIndex { get; set; } = -1;
    
    // Region
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;

    // Obtaining
    /// <summary>
    /// The stone's health. Every hit reduces UpgradeLevel + 1.
    /// For weapons, it does 10% average DMG + 1.
    /// See <see cref="StardewValley.Locations.MineShaft"/> for stone health.
    /// </summary>
    public int Health { get; set; } = 10;
    public string ItemDropped { get; set; }
    public int MinDrops { get; set; } = 1;
    public int MaxDrops { get; set; }
    public List<ExtraSpawn> ExtraItems { get; set; }
    
    // Type of resource
    /// <summary>
    /// Debris when destroying item. Can be an ItemId, or one of: coins, wood, stone, bigStone, bigWood, hay, weeds
    /// </summary>
    
    public string Debris { get; set; } = "stone";
    public string BreakingSound { get; set; } = "stoneCrack";
    public string Sound { get; set; } = "hammer";
    public int AddHay { get; set; }
    public bool SecretNotes { get; set; } = true;
    public bool Shake { get; set; } = true;
    public StatCounter CountTowards { get; set; } = StatCounter.None;

    /// <summary>
    /// Tool's class. In the case of weapons, it can also be its number.
    /// </summary>
    public string Tool { get; set; } = "Pickaxe";

    public NotifyForTool? SayWrongTool { get; set; } = null;
    /// <summary>
    /// Minimum upgrade tool should have. If a weapon, the minimum number is checked. 
    /// ("number": 10% of average damage)
    /// </summary>
    public int MinToolLevel { get; set; }
    public int Exp { get; set; }
    public string Skill { get; set; }
    internal int ActualSkill { get; set; } = -1;

    // Extra
    public List<string> ContextTags { get; set; } = null;
    public Dictionary<string, string> CustomFields { get; set; } = null;
    public LightData Light { get; set; } = null;

    public OnBehavior OnDestroy { get; set; } = null;
    /* in a future update, I migth allow to spawn these on mine floors. They might use % to replace stones, but i'd still have to figure out:
     * 1. the patch
     * 2. how to even the percentage across all ores/clumps
     * 3. if it breaks the game
     *
     * I'm guessing I can order a list starting from the one with highest chance, and make it so only a max of 50% stones are replaced. that, or set an ore to replace (so the user can decide willingly). Regardless, it'll be a future change
     * 
    * public string SpawnOnFloors { get; set; } = null;
    * public double SpawnFrequency { get; set; }
    */

    public bool IsValid(bool skipTextureCheck)
    {
        if (!skipTextureCheck && Game1.content.DoesAssetExist<Texture2D>(Texture) == false)
        {
            Log($"Couldn't find texture {Texture} for resource {Name}. Skipping.", LogLevel.Info);
            return false;
        }

        if (MaxDrops < MinDrops)
            MaxDrops = MinDrops + 1;
        
        if (Width <= 0)
        {
            Log("Resource width must be over 0. Skipping.", LogLevel.Warn);
            return false;
        }
        
        if (Height <= 0)
        {
            Log("Resource height must be over 0. Skipping.", LogLevel.Warn);
            return false;
        }

        ActualSkill = GetSkill(Skill);
        
        if (Light != null)
        {
            if (Light.Size == 0)
            {
                Log("Item light can't be size 0. Skipping.", LogLevel.Warn);
                return false;
            }
                
            if(Light.Transparency == 0)
            {
                Log("Item transparency can't be 0. Skipping.", LogLevel.Warn);
                return false;
            }
        }

        if (Health <= 0)
        {
            Log("Resource health must be over 0. Skipping.", LogLevel.Warn);
            return false;
        }
        
        if (SpriteIndex < 0)
        {
            Log("Resource index can't be negative. Skipping.", LogLevel.Warn);
            return false;
        }
        
        if(string.IsNullOrWhiteSpace(Texture))
        {
            Log("Must specify a texture for resource. Skipping.", LogLevel.Warn);
            return false;
        }
        
        if(string.IsNullOrWhiteSpace(Tool))
        {
            Log("Must specify a tool for resource. Skipping.", LogLevel.Warn);
            return false;
        }

        if (string.IsNullOrWhiteSpace(ItemDropped))
        {
            Log("Resource's dropped item is empty.", LogLevel.Warn);
            Log("The item will still be added, but this may cause issues.");
        }
        return true;
    }

    internal static int GetSkill(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return -1;
        }
        
        int actualSkill;
        
        if (int.TryParse(skill, out var intSkill))
            actualSkill = intSkill;
        if (skill.StartsWith("farm", Comparison))
            actualSkill = 0;
        else if (skill.StartsWith("fish", Comparison))
            actualSkill = 1;
        else if (skill.Equals("foraging", Comparison))
            actualSkill = 2;
        else if (skill.Equals("mining", Comparison))
            actualSkill = 3;
        else if (skill.Equals("combat", Comparison))
            actualSkill = 4;
        else if (skill.Equals("luck", Comparison))
            actualSkill = 5;
        else
            actualSkill = -1;
        
        return actualSkill;
    }

    /// <summary>
    /// Called when the user patches a vanilla resource. Removes every custom value except extra drops
    /// </summary>
    /// <param name="asInt"></param>
    public void Trim(int asInt)
    {
        /* doesn't need editing because we skip the entry
        Name = null;
        DisplayName = null;
        Description = null;
        Texture = null;
        SpriteIndex = -1;*/
        if (Additions.GeneralResource.VanillaClumps.Contains(asInt) == false)
        {
            Width = 1;
            Height = 1;
        }
        Health = -1;
        /*
        ItemDropped = null;
        MinDrops = 1;
        MaxDrops = null; */
        Debris = null;
        BreakingSound = null;
        Sound = null;
        AddHay = -1;
        SecretNotes = false;
        Shake = false;
        CountTowards = StatCounter.None;
        SayWrongTool = null;
        MinToolLevel = -1;
        Exp = 0;
        Skill = null;
        ActualSkill = -1;
        ContextTags = null;
        CustomFields = null;
        Light = null;
        Tool = "vanilla";
    }

    /*
    public List<string> GetFloorsSimplified()
    {
        var all = new List<string>();
        //removes spaces and then separates by comma
        var parsed = SpawnOnFloors.Replace(" ", "").Replace(',', ' ');
        var floors = ArgUtility.SplitBySpace(parsed);
        foreach (var value in floors)
        {
            if(string.IsNullOrWhiteSpace(value))
                continue;
            
            if (int.TryParse(value, out var isInt) && isInt < 1)
            {
                continue;
            }
            
            all.Add(value);
        }

        return all;
    }
    
    public List<int> GetFloors()
    {
        var all = new List<int>();
        //removes spaces and then separates by comma
        var parsed = SpawnOnFloors.Replace(" ", "").Replace(',', ' ');
        var floors = ArgUtility.SplitBySpace(parsed);
        foreach (var value in floors)
        {
            if(string.IsNullOrWhiteSpace(value))
                continue;
            
            if (int.TryParse(value, out var isInt))
            {
                all.Add(isInt);
                continue;
            }

            if (!value.Contains('-')) 
                continue;
            
            var split = ArgUtility.SplitBySpace(value.Replace('-', ' '));
                
            if(split.Length < 2 || int.TryParse(split[0], out var start) == false || int.TryParse(split[1], out var end) == false)
                continue;

            for (var i = start; i < end; i++)
            {
                all.Add(i);
            }
        }

        return all;
    }
    */
}