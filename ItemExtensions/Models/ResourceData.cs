using StardewModdingAPI;

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
    
    // required
    public string Name { get; set; }
    public string Texture { get; set; }
    public int SpriteIndex { get; set; } = -1;
    
    // region
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;

    public int SolidWidth { get; set; } = -1;
    
    public int SolidHeight  { get; set; } = -1;

    // obtaining
    /// <summary>
    /// The stone's health. Every hit reduces UpgradeLevel + 1.
    /// For weapons, it does 10% average DMG + 1.
    /// See <see cref="StardewValley.Locations.MineShaft"/> for stone health.
    /// </summary>
    public int Health { get; set; } = 10;
    public string Sound { get; set; } = "hammer";
    public string BreakingSound { get; set; } = "stoneCrack";
    public string Debris { get; set; } = "stone";
    public string ItemDropped { get; set; } = null;
    public int MinDrops { get; set; } = 1;
    public int MaxDrops { get; set; } = 1;
    public List<ExtraSpawn> ExtraItems { get; set; } = null;

    /// <summary>
    /// Tool's class. In the case of weapons, it can also be its number.
    /// </summary>
    public string Tool { get; set; } = null;
    /// <summary>
    /// Minimum upgrade tool should have. If a weapon, the minimum number is checked. 
    /// ("number": 10% of average damage)
    /// </summary>
    public int MinToolLevel { get; set; } = 0;
    public int Exp { get; set; } = 0;
    public string Skill { get; set; } = null;
    internal int ActualSkill { get; set; } = -1;

    //extra
    public List<string> ContextTags { get; set; } = null;
    public Dictionary<string, string> CustomFields { get; set; } = null;
    public LightData Light { get; set; } = null;

    public bool IsValid()
    {
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

        if (SolidWidth < 1)
            SolidWidth = Width;
        
        if (SolidHeight < 1)
            SolidHeight = Height;

        if (!string.IsNullOrWhiteSpace(Skill))
        {
            if (Skill.Equals("farming", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 0;
            else if (Skill.Equals("fishing", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 1;
            else if (Skill.Equals("foraging", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 2;
            else if (Skill.Equals("mining", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 3;
            else if (Skill.Equals("combat", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 4;
            else if (Skill.Equals("luck", StringComparison.OrdinalIgnoreCase))
                ActualSkill = 5;
            else
                ActualSkill = -1;
        }
        
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
}