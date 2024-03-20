using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

public class ResourceClumpPatches
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": postfixing SDV method \"ResourceClump.OnAddedToLocation\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(TerrainFeature), nameof(TerrainFeature.OnAddedToLocation)),
            postfix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Post_OnAddedToLocation))
        );
        
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": transpiling SDV method \"ResourceClump.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
            transpiler: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Transpiler))
        );
    }
    
    public static void Post_OnAddedToLocation(TerrainFeature __instance, GameLocation location, Vector2 tile)
    {
        if (__instance is not ResourceClump r)
            return;
        
        if (r.modData.TryGetValue(ModKeys.CustomClumpId, out var id) is false) 
            return;
        
        if (ModEntry.BigClumps.TryGetValue(id, out var data) == false)
        {
            Log("Clump not found.");
            return;
        }

        var light = data.Light;

        if (light is null || light == new LightData())
            return;

        var fixedPosition = new Vector2(tile.X + r.width.Value / 2, tile.Y * r.height.Value / 2);
        var lightSource = new LightSource(4, fixedPosition, light.Size, light.GetColor());

        r.modData.Add(ModKeys.LightSourceId, $"{lightSource.Identifier}");
    }
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();

        var index = codes.FindIndex(ci => ci.opcode == OpCodes.Br);
        Log($"index: {index}", LogLevel.Info);
        
        var redirectTo = codes.Find(ci => codes.IndexOf(ci) == index);
        
        //add label for brfalse
        var label = il.DefineLabel();
        redirectTo.labels ??= new List<Label>();
        redirectTo.labels.Add(label);
        
        if (index <= -1) 
            return codes.AsEnumerable();
        
        #if DEBUG
        Log($"INDEXED \nname: {codes[index].opcode.Name}, type: {codes[index].opcode.OpCodeType}, operandtype: {codes[index].opcode.OperandType}, \npop: {codes[index].opcode.StackBehaviourPop}, push: {codes[index].opcode.StackBehaviourPush}, \nvalue: {codes[index].opcode.Value}, flowcontrol: {codes[index].opcode.FlowControl}, operand: {codes[index].operand}");
        Log($"REDIRECT \nname: {redirectTo.opcode.Name}, type: {redirectTo.opcode.OpCodeType}, operandtype: {redirectTo.opcode.OperandType}, \npop: {redirectTo.opcode.StackBehaviourPop}, push: {redirectTo.opcode.StackBehaviourPush}, \nvalue: {redirectTo.opcode.Value}, flowcontrol: {redirectTo.opcode.FlowControl}, operand: {redirectTo.operand}");
        #endif
        
        /* if (IsCustom(this))
         * {
         *      return DoCustom(this);
         * }
         */
        
        //idk
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Nop));
        
        //arg: this
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
        
        //IsCustom(arg)
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtensionClump), nameof(ExtensionClump.IsCustom))));
        
        //pop value
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_3));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_3));
        
        //if false
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse, label)); 
        
        //idk
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Nop));
        
        // this, t, damage, tileLocation
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_2));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_3));
        
        //DoCustom(prev args)
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtensionClump), nameof(ExtensionClump.DoCustom))));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, 4));

        #if DEBUG
        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log("Inserting method");
        #endif
        
        codes.InsertRange(index, instructionsToInsert);
        return codes.AsEnumerable();
    }
    
}