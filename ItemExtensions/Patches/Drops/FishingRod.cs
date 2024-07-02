using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Tools;

namespace ItemExtensions.Patches;

internal class FishingRodPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level = LogLevel.Trace;
#endif

    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(FishingRodPatches)}\": transpiling game method \"FishingRod.openTreasureMenuEndFunction\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.openTreasureMenuEndFunction)),
            transpiler: new HarmonyMethod(typeof(FishingRodPatches), nameof(Transpiler_openTreasureMenuEndFunction))
        );
    }
    
    private static IEnumerable<CodeInstruction> Transpiler_openTreasureMenuEndFunction(IEnumerable<CodeInstruction> instructions)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();

        var index = codes.FindIndex(ci => ci.opcode == OpCodes.Newobj) + 1;

        var listConstructor = codes.Find(ci => ci.opcode == OpCodes.Newobj);
        var add = codes.FindAll(ci => ci.opcode == OpCodes.Callvirt)[3];
        
#if DEBUG
        Log("Index is " + index);
#endif
        /*
         * plan:
         *  first you call getallitems w/ variable
         *  then its enumerator
         *  then pop its value
         *  then br_S to "call ldloca_s for the enumerable"
         *  ldloca_S
         *  then get current from enumerator
         *  and stloc to set variable somewhere
         *  load the list
         *  load the current item
         *  call Add
         *  call ldloca_s for the enumerable
         *  call movenext
         *  if true, go back to popping value
         *  then leave_s
         */
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FishingRodPatches), nameof(GetAllItems))));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt));
        instructionsToInsert.Add(new CodeInstruction(pop_the_value));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Br_S, redirectlabel));
        instructionsToInsert.Add(new CodeInstruction(get_the_enumerator_value));
        instructionsToInsert.Add(new CodeInstruction(get_current));
        instructionsToInsert.Add(new CodeInstruction(set_variable_somewhere));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_1));
        instructionsToInsert.Add(new CodeInstruction(the_current_value));
        instructionsToInsert.Add(add);
        instructionsToInsert.Add(new CodeInstruction(ldloca_s_for_enumerable));
        instructionsToInsert.Add(new CodeInstruction(movenext));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brtrue));
        instructionsToInsert.Add(new CodeInstruction(go_back_to_pop));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Leave_S));
        /*
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0)); //player
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloca_S, 1)); //inventory
        //call w/ previous arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FishingRodPatches), nameof(AddExtraDrops))));*/
        
        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log($"Inserting method at {index}");
        codes.InsertRange(index, instructionsToInsert);

        return codes.AsEnumerable();
    }

    internal static void THIS_IS_TO_TEST_ENUMERATORS(Farmer who)
    {
        var list = new List<Item>();
        
        foreach (var item in GetAllItems(who))
        {
            list.Add(item);
        }
    }

    internal static List<Item> GetAllItems(Farmer who)
    {
        var result = new List<Item>();
#if DEBUG
        Log("Checking extra drops.", LogLevel.Warn);
#endif
        var context = new ItemQueryContext(who.currentLocation, who, Game1.random);

        foreach (var (entry, data) in ModEntry.Treasure)
        {
#if DEBUG
            Log($"Checking entry {entry}...");
#endif
            if (Sorter.GetItem(data, context, out var item) == false)
                continue;

            //PROBLEM TO FIGURE OUT: can't access inventory list
            result.Add(item);
            
            Log($"Added treasure reward from entry {entry} ({item.QualifiedItemId})");
        }
        return result;
    }
    
    internal static void AddExtraDrops(Farmer who, ref List<Item> inventory)
    {
#if DEBUG
        Log("Checking extra drops.", LogLevel.Warn);
#endif
        var context = new ItemQueryContext(who.currentLocation, who, Game1.random);

        foreach (var (entry, data) in ModEntry.Treasure)
        {
#if DEBUG
            Log($"Checking entry {entry}...");
#endif
            if (Sorter.GetItem(data, context, out var item) == false)
                continue;

            //PROBLEM TO FIGURE OUT: can't access inventory list
            inventory.Add(item);
            
            Log($"Added treasure reward from entry {entry} ({item.QualifiedItemId})");
        }
    }
}