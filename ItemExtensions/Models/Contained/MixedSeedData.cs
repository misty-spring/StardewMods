using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Models.Contained;

public class MixedSeedData
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    private static IModHelper Helper => ModEntry.Help;
    
    public string ItemId { get; set; }
    public SeasonCondition Season { get; set; } = SeasonCondition.Any;
    public string Condition { get; set; }
    public string HasMod { get; set; }
    public string NotHaveMod { get; set; }
    public int Weight { get; set; } = 1;

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(ItemId))
        {
            Log("Must specify seed name! Skipping", LogLevel.Warn);
            return false;
        }

        var items = Game1.objectData;
        
        if (items.ContainsKey(ItemId) == false)
        {
            Log("Seed doesn't seem to exist in-game.", LogLevel.Warn);
            return false;
        }

        return true;
    }

    public bool CheckConditions()
    {
        if (string.IsNullOrWhiteSpace(HasMod))
            return true;
        
        if (Season != SeasonCondition.Any)
        {
            if (Game1.season.Equals(Season) == false)
                return false;
        }

        return Helper.ModRegistry.Get(HasMod) != null;
    }
}

public enum SeasonCondition{
    /// <summary> Any season. </summary>
    Any,
    /// <summary>The spring season.</summary>
    Spring,
    /// <summary>The summer season.</summary>
    Summer,
    /// <summary>The fall season.</summary>
    Fall,
    /// <summary>The winter season.</summary>
    Winter,
}