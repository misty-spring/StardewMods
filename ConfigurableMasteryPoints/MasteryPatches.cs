using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace ConfigurableMasteryPoints;

public class MasteryPatches
{

#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif

    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    public static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(MasteryPatches)}\": postfixing SDV method \"MasteryTrackerMenu.getMasteryExpNeededForLevel\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getMasteryExpNeededForLevel)),
            postfix: new HarmonyMethod(typeof(MasteryPatches), nameof(Post_getMasteryExpNeededForLevel))
        );
    }
    
    public static void Post_getMasteryExpNeededForLevel(int level, ref int __result)
    {
        __result = level switch
        {
            0 => 0,
            1 => ModEntry.Config.Level1,
            2 => ModEntry.Config.Level1 + ModEntry.Config.Level2,
            3 => ModEntry.Config.Level1 + ModEntry.Config.Level2 + ModEntry.Config.Level3,
            4 => ModEntry.Config.Level1 + ModEntry.Config.Level2 + ModEntry.Config.Level3 + ModEntry.Config.Level4,
            5 => ModEntry.Config.Level1 + ModEntry.Config.Level2 + ModEntry.Config.Level3 + ModEntry.Config.Level4 + ModEntry.Config.Level5,
            _ => int.MaxValue
        };
    }
}