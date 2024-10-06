using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Additions.Clumps;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

public class ResourceClumpPatches
{
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
        
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": prefixing SDV method \"ResourceClump.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
            prefix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Pre_performToolAction))
        );
        
        //CJB cheats menu bugfix
        var breakCheat = AccessTools.Method($"CJBCheatsMenu.Framework.Cheats.PlayerAndTools:OneHitBreakCheat");
        if (updateAttachments is null) //if the method isn't found, return
        {
            Log($"Method not found. (OneHitBreakCheat)", LogLevel.Warn);
            return;
        }
        
        Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": postfixing CJBCheatsMenu method \"Framework.Cheats.PlayerAndTools.OneHitBreakCheat\".");
        harmony.Patch(
            original: breakCheat,
            postfix: new HarmonyMethod(typeof(ResourceClumpPatches), nameof(Post_OnUpdated))
        );
        
        /*Log($"Applying Harmony patch \"{nameof(ResourceClumpPatches)}\": transpiling SDV method \"ResourceClump.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(ToolActionTranspiler))
        );*/
    }
    
    public static void Post_OnAddedToLocation(TerrainFeature __instance, GameLocation location, Vector2 tile)
    {
        try
        {
            if (__instance is not ResourceClump r)
                return;

            //if no custom id
            if (r.modData.TryGetValue(ModKeys.ClumpId, out var id) is false)
                return;

            //if it has a light Id, assume there's one placed
            if (r.modData.TryGetValue(ModKeys.LightId, out _))
                return;

            //try get light data
            if (r.modData.TryGetValue(ModKeys.LightSize, out var sizeRaw) == false ||
                r.modData.TryGetValue(ModKeys.LightColor, out var rgb) == false ||
                r.modData.TryGetValue(ModKeys.LightTransparency, out var transRaw) == false)
            {
#if DEBUG
                ModEntry.Mon.VerboseLog($"Data for {id} light not found. (onAddedToLocation)");
#endif
                return;
            }

            if (float.TryParse(sizeRaw, out var size) == false)
            {
                Log($"Couldn't parse light size for clump Id {id} ({sizeRaw})");
                return;
            }

            if (float.TryParse(transRaw, out var trans) == false)
            {
                Log($"Couldn't parse transparency for clump Id {id} ({sizeRaw})");
                return;
            }

            //parse
            Color color;
            if (rgb.Contains(' ') == false)
            {
                color = Utility.StringToColor(rgb) ?? Color.White;
            }
            else
            {
                var rgbs = ArgUtility.SplitBySpace(rgb);
                var parsed = rgbs.Select(int.Parse).ToList();
                color = new Color(parsed[0], parsed[1], parsed[2]);
            }

            color *= trans;

            //set
            var fixedPosition = new Vector2(tile.X + r.width.Value / 2, tile.Y * r.height.Value / 2);
            var lightSource = new LightSource(4, fixedPosition, size, color);

            r.modData.Add(ModKeys.LightId, $"{lightSource.Identifier}");
        }
        catch (Exception e)
        {
            Log($"Error: {e}",LogLevel.Error);
        }
    }
    
    //the transpiler would return anyway, so we make it a prefix
    public static bool Pre_performToolAction(ref ResourceClump __instance, Tool t, int damage, Vector2 tileLocation, ref bool __result)
    {
        try
        {
            if (ExtensionClump.IsCustom(__instance) == false)
                return true;

            __result = ExtensionClump.DoCustom(ref __instance, t, damage, tileLocation);
            return false;
        }
        catch (Exception e)
        {
            Log($"Error: {e}");
            return true;
        }
    }
    
    private static IEnumerable<CodeInstruction> ToolActionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //find second ldarg.0
        //then insert: ldarg.0, ldarg.1, ldarg.2, ldarg.3, code and call. if true, return
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();

        var codeInstruction = codes.FindAll(ci => ci.opcode == OpCodes.Ldarg_0)[1];
        var index = codes.IndexOf(codeInstruction);
#if DEBUG
        Log($"index: {index}", LogLevel.Info);
#endif
        var redirectTo = codeInstruction;
        
        //add label for brfalse
        var brfalseLabel = il.DefineLabel();
        redirectTo.labels ??= new List<Label>();
        redirectTo.labels.Add(brfalseLabel);
        
        if (index <= -1) 
            return codes.AsEnumerable();
        
        /* if (DoCustom(this, damage, tool, tile))
         * {
         *      return true;
         * }
         */
        
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0)); //this
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_1)); //
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_2)); //
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_3)); //
        
        //call my code w/ prev args
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtensionClump), nameof(ExtensionClump.DoCustom))));

        //tell where to go if false
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse, brfalseLabel));
        
        //if true: ret true
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ret));
        
        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log("Inserting method");
        codes.InsertRange(index, instructionsToInsert);
        
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

    private static void Post_OnUpdated(object context, object e) //CheatContext, UpdateTickedEventArgs
    {
        //setting health to 0 bugs out IE
        // skip if not using a tool
        if (!Context.IsPlayerFree || !Game1.player.UsingTool)
            return;

        Farmer player = Game1.player;
        var location = player.currentLocation;
        if (location == null)
            return;

        foreach (ResourceClump? clump in location.resourceClumps)
        {
            if (clump != null && clump.getBoundingBox().Contains((int)player.GetToolLocation().X, (int)player.GetToolLocation().Y) && clump.health.Value == 0)
                clump.health.Value = 1;
        }
    }
}