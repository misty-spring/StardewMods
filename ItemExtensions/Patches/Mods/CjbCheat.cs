using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace ItemExtensions.Patches.Mods;

// ReSharper disable once InconsistentNaming
internal static class CjbCheat
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        //CJB cheats menu bugfix
        var breakCheat = AccessTools.Method($"CJBCheatsMenu.Framework.Cheats.PlayerAndTools.OneHitBreakCheat:OnUpdated");
        if (breakCheat is null) //if the method isn't found, return
        {
            Log($"Method not found. (OneHitBreakCheat:OnUpdated)", LogLevel.Warn);
            return;
        }
        /*
        var @params="";
        foreach (var VARIABLE in breakCheat.GetParameters())
        {
            @params += VARIABLE + ", ";
        }
        Log(@params, LogLevel.Alert);*/

        Log($"Applying Harmony patch \"{nameof(CjbCheat)}\": postfixing CJBCheatsMenu method \"Framework.Cheats.PlayerAndTools.OneHitBreakCheat:OnUpdated\".");
        harmony.Patch(
            original: breakCheat,
            //postfix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Post_OnUpdated))
            transpiler: new HarmonyMethod(typeof(CjbCheat), nameof(CJB_Transpiler))
        );
    }

    /*private static void Post_OnUpdated(CheatContext context, UpdateTickedEventArgs e)
    {
        if (!Context.IsPlayerFree || !Game1.player.UsingTool || Game1.player.CurrentTool is not (Axe or Pickaxe))
            return;
        
        var player = Game1.player;
        var location = Game1.player.currentLocation;
        if (location == null)
            return;
        
        foreach (ResourceClump? clump in location.resourceClumps)
        {
            if (clump != null && clump.getBoundingBox().Contains((int)player.GetToolLocation().X, (int)player.GetToolLocation().Y) && clump.health.Value == 0)
                clump.health.Value = 1;
        }
    }*/
    
    private static IEnumerable<CodeInstruction> CJB_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);

        var prevInstruction = codes.FindAll(ci => ci.opcode == OpCodes.Ldloca_S)[1];
        var prevIndex = codes.IndexOf(prevInstruction);
        
        var replaceThis = codes.Find(ci => codes.IndexOf(ci) == prevIndex - 1);
        var index = codes.IndexOf(replaceThis);
        var indexCallMethod = codes.Count - 1;
#if DEBUG
        Log($"index: {index}", LogLevel.Info);
#endif
        
        CodeInstruction newInstruction = replaceThis;
#if DEBUG
        Log(replaceThis?.operand?.ToString(), LogLevel.Alert);
#endif
        newInstruction.operand = 1f;
        
        /* ((NetFieldBase<float, NetFloat>)(object)clump.health).set_Value(0f);
         * to
         * ((NetFieldBase<float, NetFloat>)(object)clump.health).set_Value(1f);
         */
        
        Log("Replacing value in code");
        codes[index] = newInstruction;
        
        var lastCode = codes[indexCallMethod];
        
        Log("Inserting method");
        codes[indexCallMethod] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CjbCheat), nameof(ResetClumpHealth)));
        
        Log("Inserting ret at last index");
        codes.Add(lastCode);
        
        /* print the IL code
         * courtesy of atravita
         *
        StringBuilder sb = new();
        sb.Append("ILHelper for: GameLocation.spawnObjects");
        for (int i = 0; i < codes.Count; i++)
        {
            sb.AppendLine().Append(codes[i]);
            if (index + 3 == i)
            {
                sb.Append("       <---- start of transpiler");
            }
            if (index + 3 + instructionsToInsert.Count == i)
            {
                sb.Append("       <----- end of transpiler");
            }
        }
        Log(sb.ToString(), LogLevel.Info);
        */
        return codes.AsEnumerable();
    }

    public static void ResetClumpHealth()
    {
        var player = Game1.player;
        var location = Game1.player.currentLocation;
        if (location == null)
            return;
        
        foreach (ResourceClump? clump in location.resourceClumps)
        {
            if (clump != null && clump.getBoundingBox().Contains((int)player.GetToolLocation().X, (int)player.GetToolLocation().Y))
                clump.health.Value = 1;
        }

        return;
    }
}