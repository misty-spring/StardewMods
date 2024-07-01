using System.Reflection;
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
        
#if DEBUG
        Log("Index is " + index);
#endif
        /*
           foreach (var item in FishingRodPatches.GetAllItems(lastUser))  {
             inventory.Add(item);
           }
         */
        
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg, 0)); //lastUser
        //GetAllItems
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FishingRodPatches), nameof(GetAllItems))));
        //get enumerator
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, typeof(List<Item>).GetMethod("GetEnumerator")));
        //
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc, 10)); //lastUser
        
        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log($"Inserting method at {index}");
        codes.InsertRange(index, instructionsToInsert);

        return codes.AsEnumerable();
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