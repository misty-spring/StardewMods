using StardewModdingAPI;
using StardewValley;

namespace ItemExtensions.Models;

public class FarmerAnimation
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal FarmerSprite.AnimationFrame[] ActualAnimation { get; set; } = null;
        
    /// <summary>
    /// In file: int[] of "frame duration" (repeat).
    /// In item: Can be a name (vanilla or custom), or animation frames.
    /// </summary>
    public string Animation { get; set; } = null;

    public FoodAnimation Food { get; set; } = null;
    
    /// <summary>
    /// Mutually exclusive with <see cref="Food"/>.
    /// </summary>
    public bool HideItem { get; set; } = true;
    
    /// <summary>
    /// Message to show.
    /// </summary>
    public string ShowMessage { get; set; } = null;
    
    /// <summary>
    /// Sound to play.
    /// </summary>
    public string PlaySound { get; set; } = null;

    /// <summary>
    /// Optional sound delay.
    /// </summary>
    public int SoundDelay { get; set; } = 0;

    /// <summary>
    /// Track to change music to.
    /// </summary>
    public string PlayMusic { get; set; } = null;
    
    /// <summary>
    /// Only usable via item data.
    /// </summary>
    public string TriggerAction { get; set; } = null;

    internal bool IsValid(out FarmerAnimation result)
    {
        result = null;
        
        if (string.IsNullOrWhiteSpace(Animation))
        {
            Log("Must specify an animation.", LogLevel.Warn);
            return false;
        }
        
        if ((Food is not null || Food != new FoodAnimation()) && HideItem)
        {
            Log("Eating animation and HideItem are mutually exclusive.", LogLevel.Warn);
            return false;
        }

        try
        {
            List<FarmerSprite.AnimationFrame> realFrames = new();
            var parsed = ArgUtility.SplitBySpace(Animation);
            var toInt = parsed.Select(int.Parse).ToList();
            var skipNext = false;

            for (var i = 0; i < toInt.Count - 1; i++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                realFrames.Add(new FarmerSprite.AnimationFrame(toInt[i], toInt[i + 1]));
                skipNext = true;
            }

            ActualAnimation = realFrames.ToArray();
            result = this;

            return true;
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
            return false;
        }
    }
}