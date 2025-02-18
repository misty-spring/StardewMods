using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace FrogDropsLoot;

public class ModEntry : Mod
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => Mon.Log(msg, lv);
    private static IMonitor Mon { get; set; }
    private static IModHelper Help { get; set; }
    private static Dictionary<string,List<string>> CachedCategories { get; set; } = new();
    
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.ReturnedToTitle += OnTitleReturn;
        
        Mon = Monitor;
        Help = Helper;
        
        var harmony = new Harmony(ModManifest.UniqueID);
        Monitor.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": transpiling SDV method \"HungryFrogCompanion.Update\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(HungryFrogCompanion), nameof(HungryFrogCompanion.Update)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Transpiler))
        );
    }

    private void OnTitleReturn(object? sender, ReturnedToTitleEventArgs e)
    {
        CachedCategories.Clear();
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();
        
        CodeInstruction removeMonster = null;
        CodeInstruction getMonster = null;
        var insertAt = -1;
        var insertAgain = -1;
        for (var i = 2; i < codes.Count - 1; i++)
        {
            if(codes[i - 1].opcode != OpCodes.Callvirt)
                continue;

            if(codes[i].opcode != OpCodes.Pop)
                continue;
            
            if(codes[i + 1].opcode != OpCodes.Br_S)
                continue;

            removeMonster = codes[i];
            insertAt = i - 7;
            getMonster = codes[i - 2];
            insertAgain = i + 2;
            break;
        }
#if DEBUG
        Log($"index: {insertAt}", LogLevel.Info);
#endif
        if (removeMonster is null || getMonster is null || insertAt < 0 || insertAgain < 0)
        {
            Log("Couldn't find \"this.attachedMonster.currentLocation.characters.Remove(this.attachedMonster)\".");
            return codes.AsEnumerable();
        }
        
        /* DropLoot(this); */
        
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0)); //this
        instructionsToInsert.Add(getMonster); //getMonster
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(DropLoot))));

        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log("Inserting method");
        codes.InsertRange(insertAt, instructionsToInsert);
        codes.InsertRange(insertAgain, instructionsToInsert);
        
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

    public static void DropLoot(Monster attachedMonster)
    {
        if (attachedMonster is null)
        {
#if DEBUG
            Log("Attached Monster is null.");
#endif
            return;
        }
        foreach (var item in attachedMonster.objectsToDrop)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;

            var vector = attachedMonster.Position;
            
            var vector2 = Game1.random.Next(4) switch
            {
                0 => new Vector2(-64f, 0f),
                1 => new Vector2(64f, 0f),
                2 => new Vector2(0f, 64f),
                _ => new Vector2(0f, -64f),
            };
            
            var item2 = item.StartsWith('-') ? GetItemFromCategory(item) : ItemRegistry.Create(item);
            attachedMonster.currentLocation.debris.Add(new Debris(item2, vector, vector + vector2));
        }
    }

    private static Item GetItemFromCategory(string category)
    {
        if (CachedCategories.TryGetValue(category, out var allItems))
        {
            return ItemRegistry.Create(Game1.random.ChooseFrom(allItems));
        }
        
        var items = new List<string>();
        foreach (var objectData in Game1.objectData)
        {
            if (int.TryParse(category, out var cat) == false)
                continue;
            
            if (objectData.Value.Category.Equals(cat))
                items.Add(objectData.Key);
        }

        CachedCategories.TryAdd(category, items);
        
        return ItemRegistry.Create(Game1.random.ChooseFrom(items));
    }
}